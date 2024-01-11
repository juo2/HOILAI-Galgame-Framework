using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BoundsObject : MonoBehaviour
{
    public Bounds bounds;
    public void Start()
    {
#if UNITY_EDITOR
        if(bounds.extents.magnitude <= 0.01)
        {
            Renderer[] childs = gameObject.GetComponentsInChildren<Renderer>();

            Vector3 center = Vector3.zero;

            if (childs.Length > 0)
            {
                foreach (var item in childs)
                    center += item.bounds.center;
                center /= childs.Length;
            }

            bounds = new Bounds(center, Vector3.zero);
            foreach (var item in childs)
                bounds.Encapsulate(item.bounds);
        }

#endif
    }

    void OnDrawGizmosSelected()
    {
        Color c = Gizmos.color;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
        Gizmos.color = c;
    }
}
