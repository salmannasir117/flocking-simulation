using System.Collections;
using System.Collections.Generic;
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
        float max_vel = 1.5f, min_vel = -1.5f;
        for (int i = 0; i < num_boids; i++) {
            Vector3 rand_velocity = new Vector3(Random.value * (max_vel - min_vel) + min_vel, 0, Random.value * (max_vel - min_vel) + min_vel);
            boids[i] = new Boid(new Vector3(0,0,0), rand_velocity);
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
        
        foreach (Boid boid in boids) {
            if (boid == null) continue;
            boid.update_position(dt);
            boid.draw();
        } 

        // test_boid.update_position(0.01f);
        // test_boid.draw();
        // test_boid.update_position(test_position);
        // test_position += new Vector3(0.0f, 0.0f, 0.001f);
        // Debug.Log(test_position);

    }
}
