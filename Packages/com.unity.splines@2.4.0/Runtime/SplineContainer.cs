using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines
{
    /// <summary>
    /// A component that holds a <see cref="Spline"/> object.
    /// </summary>
#if UNITY_2021_2_OR_NEWER
    [Icon(k_IconPath)]
#endif
    [AddComponentMenu("Splines/Spline")]
    [ExecuteInEditMode]
    public sealed class SplineContainer : MonoBehaviour, ISplineContainer, ISerializationCallbackReceiver
    {
        const string k_IconPath = "Packages/com.unity.splines/Editor/Resources/Icons/SplineComponent.png";

        // Keeping a main spline to be backwards compatible with older versions of the spline package
        [SerializeField, Obsolete, HideInInspector]
        Spline m_Spline;

        [SerializeField]
        Spline[] m_Splines = { new Spline() };

        [SerializeField]
        KnotLinkCollection m_Knots = new KnotLinkCollection();

        List<(int previousIndex, int newIndex)> m_ReorderedSplinesIndices = new List<(int, int)>();
        List<int> m_RemovedSplinesIndices = new List<int>();
        List<int> m_AddedSplinesIndices = new List<int>();

        List<GameObject>[] m_WorldKnots;


        private void Awake()
        {
            //GameObject worldKnot = Resources.Load<GameObject>("Prefabs/Rest/WorldKnot");
            //m_WorldKnots = new List<GameObject>[Splines.Count];
            //for (int i = 0; i < Splines.Count; i++)
            //{
            //    m_WorldKnots[i] = new List<GameObject>();
            //    for (int j = 0; j < Splines[i].Count; j++)
            //    {
            //        m_WorldKnots[i].Add(Instantiate(worldKnot, transform.TransformPoint(Splines[i].Knots.ToArray()[j].Position), Quaternion.identity, transform));
            //        m_WorldKnots[i].Last().transform.localRotation = Splines[i].Knots.ToArray()[j].Rotation;
            //    }
            //}

            m_WorldKnots = new List<GameObject>[Splines.Count];
            int k = 0;
            for (int i = 0; i < m_WorldKnots.Length; i++)
            {
                m_WorldKnots[i] = new List<GameObject>();
                if (transform.childCount > 0)
                    for (int j = 0; j < Splines[i].Knots.ToArray().Length && k < transform.childCount; j++)
                    {
                        Transform worldKnot = transform.GetChild(k++);
                        m_WorldKnots[i].Add(worldKnot.gameObject);
                        //if (j == Splines[i].Knots.ToArray().Length - 1) { worldKnot.position += -worldKnot.forward * 0.25f; }
                        //else if (j == 0) { worldKnot.position += worldKnot.forward * 0.25f; }
                    }
            }
            //try { SplineBuilder(transform.parent.GetComponent<SplineBuilderTool>()); }
            //catch (NullReferenceException) { }


        }

        private void SplineBuilder(SplineBuilderTool builder)
        {
            if (builder != null)
            {
                if (builder.shape == SplineBuilderTool.Shape.Curved)
                {
                    for (int r = 0; r < builder.lanes.Count; r++)
                    {
                        float radius = builder.lanes[r];
                        float step = (Mathf.PI / 16) * builder.lanes[r];
                        Spline spline = this.AddSpline();
                        for (float angle = (Mathf.PI / 2) * (builder.quater - 1); angle <= Mathf.PI / 2 * builder.quater; angle += 4 / radius)
                        {
                            float x = Mathf.Cos(angle) * radius + builder.offset.x;
                            float z = Mathf.Sin(angle) * radius + builder.offset.z;
                            BezierKnot knot = new BezierKnot(new float3(x, 0, z), 0, 0, quaternion.identity);
                            spline.Add(knot);
                            if (angle < (Mathf.PI / 2) * (builder.quater - 1) + (Mathf.PI / 4)) { radius += builder.curvatureDif[r] / step; }
                            else { radius -= builder.curvatureDif[r] / step; }
                        }
                        spline.SetTangentMode(TangentMode.AutoSmooth);
                    }
                }
                else if (builder.shape == SplineBuilderTool.Shape.Straight)
                {
                    for (int l = 0; l < builder.lanes.Count; l++)
                    {
                        Spline spline = this.AddSpline();
                        for (int i = -builder.length; i <= 0; i += 4)
                        {
                            BezierKnot knot = new BezierKnot(new float3(builder.lanes[l], 0, i), 0, 0, quaternion.identity);
                            spline.Add(knot);
                        }
                        spline.SetTangentMode(TangentMode.AutoSmooth);
                    }
                }
                else
                {
                    for (int r = 0; r < builder.lanes.Count; r++)
                    {
                        float radius = builder.lanes[r];
                        float step = (Mathf.PI / 16) * builder.lanes[r];
                        Spline spline = this.AddSpline();
                        for (float angle = 0; angle < 2 * Mathf.PI; angle += 4 / radius)
                        {
                            float x = Mathf.Cos(angle) * radius + builder.offset.x;
                            float z = Mathf.Sin(angle) * radius + builder.offset.z;
                            BezierKnot knot = new BezierKnot(new float3(x, 0, z), 0, 0, quaternion.identity);
                            spline.Add(knot);
                            if (angle < (Mathf.PI / 2) * (builder.quater - 1) + (Mathf.PI / 4)) { radius += builder.curvatureDif[r] / step; }
                            else { radius -= builder.curvatureDif[r] / step; }
                        }
                        spline.SetTangentMode(TangentMode.AutoSmooth);
                    }
                }
            }
        }

        public void SliceFromKnot(GameObject knot, Transform consumer, SplineContainer splCon)
        {
            for (int i = 0; i < m_WorldKnots.Length; i++)
            {
                if (m_WorldKnots[i].Contains(knot))
                {
                    int knotInd = m_WorldKnots[i].IndexOf(knot);
                    splCon.transform.position = transform.parent.position;
                    splCon.transform.rotation = transform.parent.rotation;
                    splCon.Spline = new Spline(Splines[i]);
                    if (splCon.Spline.Closed) { splCon.Spline.Closed = false; }

                    if (Mathf.Abs(Vector3.Angle(consumer.forward, knot.transform.forward)) > 90)
                    {
                        while (knotInd + 1 < splCon.Spline.Count) { splCon.Spline.RemoveAt(knotInd + 1); }
                        splCon.ReverseFlow(0);
                    }
                    else
                        for (int j = 0; j < knotInd; j++) { splCon.Spline.RemoveAt(0); }

                    BezierKnot secondKnot = new BezierKnot(splCon.Spline.ElementAt(0).Position, new float3(0, 0, -10f), splCon.Spline.ElementAt(0).TangentOut, splCon.Spline.ElementAt(0).Rotation);
                    splCon.Spline.SetKnot(0, secondKnot);
                    consumer.parent = transform.parent;
                    BezierKnot firstKnot = new BezierKnot(transform.parent.InverseTransformPoint(consumer.position), 0, new float3(0, 0, 10f), consumer.localRotation);
                    consumer.parent = null;
                    splCon.Spline.Insert(0, firstKnot);

                    break;
                }
            }
        }

        public void MatchLanes(Vector3 consumerPos, SplineContainer splCon)
        {
            int closestSplineIndex = 0;
            float closestKnotDist = Mathf.Infinity;
            bool reverseFlow = false;

            for (int i = 0; i < Splines.Count; i++)
            {
                float firstKnot = (consumerPos - m_WorldKnots[i][0].transform.position).magnitude;
                float lastKnot = (consumerPos - m_WorldKnots[i][m_WorldKnots[i].Count - 1].transform.position).magnitude;
                float closest = Mathf.Min(firstKnot, lastKnot);
                if (closest < closestKnotDist) { closestKnotDist = closest; closestSplineIndex = i; reverseFlow = closest == lastKnot; };
            }

            splCon.Spline = new Spline(Splines[closestSplineIndex]);
            if (reverseFlow) { splCon.ReverseFlow(0); }
        }

        /// <summary>
        /// Invoked any time a spline is added to the container.
        /// </summary>
        /// <remarks>
        /// The parameter corresponds to the spline index.
        /// </remarks>
        public static event Action<SplineContainer, int> SplineAdded;

        /// <summary>
        /// Invoked any time a spline is removed from the container.
        /// </summary>
        /// <remarks>
        /// The parameter corresponds to the spline index.
        /// </remarks>
        public static event Action<SplineContainer, int> SplineRemoved;

        /// <summary>
        /// Invoked any time a spline is reordered in the container.
        /// </summary>
        /// <remarks>
        /// The first parameter corresponds to the previous spline index,
        /// the second parameter corresponds to the new spline index.
        /// </remarks>
        public static event Action<SplineContainer, int, int> SplineReordered;

        /// <summary>
        /// Reverses splines' flow if needed to match consumers' movement direction
        /// </summary>
        /// <param name="consumerWorldLocation"></param>

        /// <summary>
        /// The list of all splines attached to that container.
        /// </summary>
        public IReadOnlyList<Spline> Splines
        {
            get => new ReadOnlyCollection<Spline>(m_Splines);
            set
            {
                if (value == null)
                {
                    m_Splines = new Spline[0];
                    return;
                }

                m_ReorderedSplinesIndices.Clear();
                m_RemovedSplinesIndices.Clear();
                m_AddedSplinesIndices.Clear();

                for (var i = 0; i < m_Splines.Length; i++)
                {
                    var index = IndexOf(value, m_Splines[i]);
                    if (index == -1)
                        m_RemovedSplinesIndices.Add(i);
                    else if (index != i)
                        m_ReorderedSplinesIndices.Add((i, index));
                }

                for (var i = 0; i < value.Count; i++)
                {
                    var index = Array.FindIndex(m_Splines, spline => spline == value[i]);
                    if (index == -1)
                        m_AddedSplinesIndices.Add(i);
                }

                m_Splines = new Spline[value.Count];
                for (int i = 0; i < m_Splines.Length; ++i)
                    m_Splines[i] = value[i];

                foreach (var removedIndex in m_RemovedSplinesIndices)
                    SplineRemoved?.Invoke(this, removedIndex);

                foreach (var addedIndex in m_AddedSplinesIndices)
                    SplineAdded?.Invoke(this, addedIndex);

                foreach (var reorderedSpline in m_ReorderedSplinesIndices)
                    SplineReordered?.Invoke(this, reorderedSpline.previousIndex, reorderedSpline.newIndex);
            }
        }

        static int IndexOf(IReadOnlyList<Spline> self, Spline elementToFind)
        {
            for (var i = 0; i < self.Count; i++)
            {
                var element = self[i];
                if (element == elementToFind)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// A collection of all linked knots. Linked knots can be on different splines. However, knots can
        /// only link to other knots within the same container. This collection is used to maintain
        /// the validity of the links when operations such as knot insertions or removals are performed on the splines.
        /// </summary>
        public KnotLinkCollection KnotLinkCollection => m_Knots;

        /// <summary>
        /// Gets or sets the <see cref="Spline"/> at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public Spline this[int index] => m_Splines[index];

        void OnEnable()
        {
            Spline.Changed += OnSplineChanged;
        }

        void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
        }

        void OnSplineChanged(Spline spline, int index, SplineModification modificationType)
        {
            var splineIndex = Array.IndexOf(m_Splines, spline);
            if (splineIndex < 0)
                return;

            switch (modificationType)
            {
                case SplineModification.KnotModified:
                    this.SetLinkedKnotPosition(new SplineKnotIndex(splineIndex, index));
                    break;

                case SplineModification.KnotReordered:
                case SplineModification.KnotInserted:
                    m_Knots.KnotInserted(splineIndex, index);
                    break;

                case SplineModification.KnotRemoved:
                    m_Knots.KnotRemoved(splineIndex, index);
                    break;
            }
        }

        void OnKnotModified(Spline spline, int index)
        {
            var splineIndex = Array.IndexOf(m_Splines, spline);
            if (splineIndex >= 0)
                this.SetLinkedKnotPosition(new SplineKnotIndex(splineIndex, index));
        }

        bool IsScaled => transform.lossyScale != Vector3.one;

        /// <summary>
        /// The main <see cref="Spline"/> attached to this component.
        /// </summary>
        public Spline Spline
        {
            get => m_Splines.Length > 0 ? m_Splines[0] : null;
            set
            {
                if (m_Splines.Length > 0)
                    m_Splines[0] = value;
            }
        }

        /// <summary>
        /// Computes interpolated position, direction and upDirection at ratio t. Calling this method to get the
        /// 3 vectors is faster than calling independently EvaluateSplinePosition, EvaluateSplineTangent and EvaluateSplineUpVector
        /// for the same time t as it reduces some redundant computation.
        /// </summary>
        /// <param name="t">A value between 0 and 1 representing the ratio along the curve.</param>
        /// <param name="position">The output variable for the float3 position at t.</param>
        /// <param name="tangent">The output variable for the float3 tangent at t.</param>
        /// <param name="upVector">The output variable for the float3 up direction at t.</param>
        /// <returns>Boolean value, true if a valid set of output variables as been computed.</returns>
        public bool Evaluate(float t, out float3 position, out float3 tangent, out float3 upVector)
            => Evaluate(0, t, out position, out tangent, out upVector);

        /// <summary>
        /// Computes the interpolated position, direction and upDirection at ratio t for the spline at index `splineIndex`. Calling this method to get the
        /// 3 vectors is faster than calling independently EvaluateSplinePosition, EvaluateSplineTangent and EvaluateSplineUpVector
        /// for the same time t as it reduces some redundant computation.
        /// </summary>
        /// <param name="splineIndex">The index of the spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 that represents the ratio along the curve.</param>
        /// <param name="position">The output variable for the float3 position at t.</param>
        /// <param name="tangent">The output variable for the float3 tangent at t.</param>
        /// <param name="upVector">The output variable for the float3 up direction at t.</param>
        /// <returns>True if a valid set of output variables is computed and false otherwise.</returns>
        public bool Evaluate(int splineIndex, float t, out float3 position,  out float3 tangent,  out float3 upVector)
            => Evaluate(Splines[splineIndex], t, out position, out tangent, out upVector);

        /// <summary>
        /// Gets the interpolated position, direction, and upDirection at ratio t for a spline.  This method gets the three
        /// vectors faster than EvaluateSplinePosition, EvaluateSplineTangent and EvaluateSplineUpVector for the same
        /// time t, because it reduces some redundant computation.
        /// </summary>
        /// <typeparam name="T">The spline type.</typeparam>
        /// <param name="spline">The spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 that represents the ratio along the curve.</param>
        /// <param name="position">The output variable for the float3 position at t.</param>
        /// <param name="tangent">The output variable for the float3 tangent at t.</param>
        /// <param name="upVector">The output variable for the float3 up direction at t.</param>
        /// <returns>True if a valid set of output variables is computed and false otherwise.</returns>
        public bool Evaluate<T>(T spline, float t, out float3 position, out float3 tangent, out float3 upVector) where T : ISpline
        {
            if (spline == null)
            {
                position = float3.zero;
                tangent = new float3(0, 0, 1);
                upVector = new float3(0, 1, 0);
                return false;
            }

            if (IsScaled)
            {
                using var nativeSpline = new NativeSpline(spline, transform.localToWorldMatrix);
                return SplineUtility.Evaluate(nativeSpline, t, out position, out tangent, out upVector);
            }

            var evaluationStatus = SplineUtility.Evaluate(spline, t, out position, out tangent, out upVector);
            if (evaluationStatus)
            {
                position = transform.TransformPoint(position);
                tangent = transform.TransformVector(tangent);
                upVector = transform.TransformDirection(upVector);
            }

            return evaluationStatus;
        }

        /// <summary>
        /// Evaluates the position of a point, t, on a spline in world space.
        /// </summary>
        /// <param name="t">A value between 0 and 1 representing a percentage of the curve.</param>
        /// <returns>A tangent vector.</returns>
        public float3 EvaluatePosition(float t) => EvaluatePosition(0, t);

        /// <summary>
        /// Evaluates the position of a point, t, on a spline at an index, `splineIndex`, in world space.
        /// </summary>
        /// <param name="splineIndex">The index of the spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of the curve.</param>
        /// <returns>A world position along the spline.</returns>
        public float3 EvaluatePosition(int splineIndex, float t) => EvaluatePosition(Splines[splineIndex], t);

        /// <summary>
        /// Evaluates the position of a point, t, on a given spline, in world space.
        /// </summary>
        /// <typeparam name="T">The spline type.</typeparam>
        /// <param name="spline">The spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of the curve.</param>
        /// <returns>A world position along the spline.</returns>
        public float3 EvaluatePosition<T>(T spline, float t) where T : ISpline
        {
            if (spline== null)
                return float.PositiveInfinity;

            if (IsScaled)
            {
                using var nativeSpline = new NativeSpline(spline, transform.localToWorldMatrix);
                return SplineUtility.EvaluatePosition(nativeSpline, t);
            }

            return transform.TransformPoint(SplineUtility.EvaluatePosition(spline, t));
        }

        /// <summary>
        /// Evaluates the tangent vector of a point, t, on a spline in world space.
        /// </summary>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed tangent vector.</returns>
        public float3 EvaluateTangent(float t) => EvaluateTangent(0, t);

        /// <summary>
        /// Evaluates the tangent vector of a point, t, on a spline at an index, `splineIndex`, in world space.
        /// </summary>
        /// <param name="splineIndex">The index of the spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed tangent vector.</returns>
        public float3 EvaluateTangent(int splineIndex, float t) => EvaluateTangent(Splines[splineIndex], t);

        /// <summary>
        /// Evaluates the tangent vector of a point, t, on a given spline, in world space.
        /// </summary>
        /// <typeparam name="T">The spline type.</typeparam>
        /// <param name="spline">The spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed tangent vector.</returns>
        public float3 EvaluateTangent<T>(T spline, float t) where T : ISpline
        {
            if (spline == null)
                return float.PositiveInfinity;

            if (IsScaled)
            {
                using var nativeSpline = new NativeSpline(spline, transform.localToWorldMatrix);
                return SplineUtility.EvaluateTangent(nativeSpline, t);
            }
            return transform.TransformVector(SplineUtility.EvaluateTangent(spline, t));
        }

        /// <summary>
        /// Evaluates the up vector of a point, t, on a spline in world space.
        /// </summary>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed up direction.</returns>
        public float3 EvaluateUpVector(float t) => EvaluateUpVector(0, t);

        /// <summary>
        /// Evaluates the up vector of a point, t, on a spline at an index, `splineIndex`, in world space.
        /// </summary>
        /// <param name="splineIndex">The index of the Spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed up direction.</returns>
        public float3 EvaluateUpVector(int splineIndex, float t) => EvaluateUpVector(Splines[splineIndex], t);

        /// <summary>
        /// Evaluates the up vector of a point, t, on a given spline, in world space.
        /// </summary>
        /// <typeparam name="T">The spline type.</typeparam>
        /// <param name="spline">The Spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed up direction.</returns>
        public float3 EvaluateUpVector<T>(T spline, float t) where T : ISpline
        {
            if (spline == null)
                return float3.zero;
            
            if (IsScaled)
            {
                using var nativeSpline = new NativeSpline(spline, transform.localToWorldMatrix, true);
                return SplineUtility.EvaluateUpVector(nativeSpline, t);
            }
            
            //Using TransformDirection as up direction is not sensible to scale.
            return transform.TransformDirection(SplineUtility.EvaluateUpVector(spline, t));
        }


        /// <summary>
        /// Evaluates the acceleration vector of a point, t, on a spline in world space.
        /// </summary>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed acceleration vector.</returns>
        public float3 EvaluateAcceleration(float t) => EvaluateAcceleration(0, t);

        /// <summary>
        /// Evaluates the acceleration vector of a point, t, on a spline at an index, `splineIndex,  in world space.
        /// </summary>
        /// <param name="splineIndex">The index of the spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed acceleration vector.</returns>
        public float3 EvaluateAcceleration(int splineIndex, float t) => EvaluateAcceleration(Splines[splineIndex], t);

        /// <summary>
        /// Evaluates the acceleration vector of a point, t, on a given Spline,  in world space.
        /// </summary>
        /// <typeparam name="T">The spline type.</typeparam>
        /// <param name="spline">The Spline to evaluate.</param>
        /// <param name="t">A value between 0 and 1 representing a percentage of entire spline.</param>
        /// <returns>The computed acceleration vector.</returns>
        public float3 EvaluateAcceleration<T>(T spline, float t) where T : ISpline
        {
            if (spline == null)
                return float3.zero;

            if (IsScaled)
            {
                using var nativeSpline = new NativeSpline(spline, transform.localToWorldMatrix);
                return SplineUtility.EvaluateAcceleration(nativeSpline, t);
            }

            return transform.TransformVector(SplineUtility.EvaluateAcceleration(spline, t));
        }

        /// <summary>
        /// Calculate the length of <see cref="Spline"/> in world space.
        /// </summary>
        /// <returns>The length of <see cref="Spline"/> in world space</returns>
        public float CalculateLength() => CalculateLength(0);

        /// <summary>
        /// Calculates the length of `Splines[splineIndex]` in world space.
        /// </summary>
        /// <param name="splineIndex">The index of the spline to evaluate.</param>
        /// <returns>The length of `Splines[splineIndex]` in world space</returns>
        public float CalculateLength(int splineIndex)
        {
            return SplineUtility.CalculateLength(Splines[splineIndex], transform.localToWorldMatrix);
        }

        /// <summary>
        /// See ISerializationCallbackReceiver.
        /// </summary>
        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// See ISerializationCallbackReceiver.
        /// </summary>
        public void OnAfterDeserialize()
        {
#pragma warning disable 612, 618
            if (m_Spline != null && m_Spline.Count > 0)
            {
                if (m_Splines == null || m_Splines.Length == 0 || m_Splines.Length == 1 && m_Splines[0].Count == 0)
                    m_Splines = new[] { m_Spline };

                m_Spline = new Spline(); //Clear spline
            }
#pragma warning restore 612, 618
        }
    }
}
