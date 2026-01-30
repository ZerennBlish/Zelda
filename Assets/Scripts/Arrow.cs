using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    public int damage = 1;
    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Bat bat = other.GetComponent<Bat>();
            if (bat != null)
            {
                bat.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            BoomShroom shroom = other.GetComponent<BoomShroom>();
            if (shroom != null)
            {
                shroom.Explode();
                Destroy(gameObject);
                return;
            }
            
            Slime slime = other.GetComponent<Slime>();
            if (slime != null)
            {
                slime.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            SlimeSplitter splitter = other.GetComponent<SlimeSplitter>();
            if (splitter != null)
            {
                splitter.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            GoblinArcher archer = other.GetComponent<GoblinArcher>();
            if (archer != null)
            {
                archer.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            GoblinMaceman maceman = other.GetComponent<GoblinMaceman>();
            if (maceman != null)
            {
                maceman.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            GoblinSpearman spearman = other.GetComponent<GoblinSpearman>();
            if (spearman != null)
            {
                spearman.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            GoblinThief thief = other.GetComponent<GoblinThief>();
            if (thief != null)
            {
                thief.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            SkeletonMage mage = other.GetComponent<SkeletonMage>();
            if (mage != null)
            {
                mage.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            ShieldKnight knight = other.GetComponent<ShieldKnight>();
            if (knight != null)
            {
                knight.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }
            
            FlyingSkull skull = other.GetComponent<FlyingSkull>();
            if (skull != null)
            {
                skull.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}