using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] float despawnZ = -10f;
    HazardPool pool;

    public Hazard Init(HazardPool p) { pool = p; return this; }

    void Update()
    {
        float speed = GameManager.Instance.Speed;
        transform.Translate(0f, 0f, -speed * Time.deltaTime, Space.World);
        if (transform.position.z < despawnZ) Despawn();
    }

    public void Despawn() => pool.Release(this);
}