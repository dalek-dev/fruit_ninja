using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using DG.Tweening;
using UnityEngine.InputSystem;


public class SliceObject : MonoBehaviour
{

    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public Material crossSectionMaterial;
    public LayerMask layerMask;
    public float cutForce = 1;

    ParticleSystem[] particles;

    // Start is called before the first frame update
    void Start()
    {
       
      
    }

    // Update is called once per frame
    void Update()
    {
        


        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, layerMask);

        if (hasHit)
        {

            Slice(true);

        }
        else
        {
            Slice(false);
        }
    }

    public void Slice(bool state)
    {
        RaycastHit[] hits = Physics.RaycastAll(startSlicePoint.position, (endSlicePoint.position - startSlicePoint.position).normalized, Vector3.Distance(startSlicePoint.position, endSlicePoint.position), layerMask);


        if (hits.Length <= 0)
            return;

        float timeScale = state ? .2f : 1;
        
        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 velocity = velocityEstimator.GetVelocityEstimate();
            Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
            planeNormal.Normalize();
            SlicedHull hull = hits[i].transform.gameObject.Slice(hits[i].transform.gameObject.transform.position, planeNormal);
            if (hull != null)
            {
                GameObject bottom = hull.CreateLowerHull(hits[i].transform.gameObject, crossSectionMaterial);
                GameObject top = hull.CreateUpperHull(hits[i].transform.gameObject, crossSectionMaterial);
                DOVirtual.Float(Time.timeScale, timeScale, .02f, SetTimeScale);
                AddHullComponents(bottom);
                AddHullComponents(top);
                Destroy(hits[i].transform.gameObject);
            }
        }


    }

    public void SetTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }

    public void AddHullComponents(GameObject slicedObject)
    {
        slicedObject.layer = 9;
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
    }

}
