using UnityEngine;

public class Main : MonoBehaviour
{
    
    public bool flock_centering, velocity_matching, collision_avoidance, wandering, leave_trail;
    Boid test_boid;
    Vector3 test_position;
    Boid[] boids;
    const int num_boids = 20;
    const float dt = 0.005f;
    // Start is called before the first frame update
    void Start()
    {
        // boids = new Boid[num_boids];
        // test_boid = new Boid(new Vector3(0,0,0), new Vector3(0.01f,0,0.01f));
        // test_position = new Vector3(0,0,0);
        // boids[0] = test_boid;

        boids = new Boid[num_boids];
        float max_vel = 1.0f, min_vel = -1.0f;
        float max_pos = 4.0f, min_pos = -4.0f;
        for (int i = 0; i < num_boids; i++) {
            Vector3 rand_velocity = new Vector3(random_from(min_vel, max_vel), 0, random_from(min_vel, max_vel));
            Vector3 rand_pos = new Vector3(random_from(min_pos, max_pos), 0, random_from(min_pos, max_pos));
            boids[i] = new Boid(rand_pos, rand_velocity);
        }
    }

     void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(0,0,0), 0.1f);
        Gizmos.DrawSphere(new Vector3(-1,0,0.75f), 0.1f);
        Gizmos.DrawSphere(new Vector3(1,0,0.75f), 0.1f);
        
        
    }
    // Update is called once per frame
    void Update()
    {
        
        //calculate forces
        foreach (Boid boid in boids) {
            //wander force 
            if (wandering) {
                Vector3 wander_force = new Vector3(random_from(-1, 1), 0, random_from(-1, 1));
                boid.set_wander_force(wander_force);
            }
        }
        
        //update velocity and position 
        foreach (Boid boid in boids) {
            boid.update_velocity(dt);
            boid.update_position(dt);
        }
        
        //draw 
        foreach (Boid boid in boids) {
            if (boid == null) continue;
            boid.draw();
        } 

        // test_boid.update_position(0.01f);
        // test_boid.draw();
        // test_boid.update_position(test_position);
        // test_position += new Vector3(0.0f, 0.0f, 0.001f);
        // Debug.Log(test_position);

    }

    float random_from(float min, float max) {
        return Random.value * (max - min) + min;
    }
}