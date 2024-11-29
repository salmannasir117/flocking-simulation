using UnityEditor;
using UnityEngine;

public class Main : MonoBehaviour
{
    
    public bool flock_centering, velocity_matching, collision_avoidance, wandering, leave_trail;
    Boid test_boid;
    Vector3 test_position;
    Boid[] boids;
    const int num_boids = 20;
    const float dt = 0.005f;
    public float speed_multiplier = 1;

    const float west_wall = -52.0f, east_wall = 52.0f, north_wall = 29.0f, south_wall = -29.0f;
    const float min_speed = 0.15f, max_speed = 1.5f;
    const float flock_radius = 2.0f, collision_radius = 1.5f, velocity_matching_radius = 1.2f;
    // Start is called before the first frame update
    
    //quick and dirty to keep these public for scatter function to see, for initial random conditions
    float max_vel = 1.0f, min_vel = -1.0f;
    float max_z_pos = north_wall - 1.0f, min_z_pos = south_wall + 1.0f;
    float max_x_pos = east_wall - 1.0f, min_x_pos = west_wall + 1.0f;
        
    void Start()
    {
        // boids = new Boid[num_boids];
        // test_boid = new Boid(new Vector3(0,0,0), new Vector3(0.01f,0,0.01f));
        // test_position = new Vector3(0,0,0);
        // boids[0] = test_boid;

        boids = new Boid[num_boids];
        for (int i = 0; i < num_boids; i++) {
            Vector3 rand_velocity = new Vector3(random_from(min_vel, max_vel), 0, random_from(min_vel, max_vel));
            Vector3 rand_pos = new Vector3(random_from(min_x_pos, max_x_pos), 0, random_from(min_z_pos, max_z_pos));
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
        if (Input.GetKeyDown(KeyCode.Space)) {
            scatter(boids);
        }
        //calculate forces
        foreach (Boid boid in boids) {
            //wander force 
            if (wandering) {
                Vector3 wander_force = new Vector3(random_from(-1, 1), 0, random_from(-1, 1));
                boid.set_wander_force(wander_force);
            } else {
                boid.set_wander_force(new Vector3(0,0,0));
            }
            if (leave_trail) {
                boid.enable_trail();
            } else {
                boid.disable_trail();
            }

            //interate, get each individual force.
            //if (not enabled), set force to zero
            if (flock_centering) {
                // List<Boid> flock_neighbors = find_flock_neighbors(boids, boid, flock_radius);
                Vector3 flocking_force = find_flocking_force(boids, boid, flock_radius);
                boid.set_flocking_force(flocking_force);
            } else {
                boid.set_flocking_force(new Vector3(0,0,0));
            } 

            if (collision_avoidance) {
                Vector3 collision_force = find_collision_force(boids, boid, collision_radius);
                boid.set_collision_force(collision_force);
            } else {
                boid.set_collision_force(new Vector3(0,0,0));
            }

            if (velocity_matching) {
                Vector3 velocity_matching_force = find_velocity_matching_force(boids, boid, velocity_matching_radius);
                boid.set_velocity_matching_force(velocity_matching_force);
            } else {
                boid.set_velocity_matching_force(new Vector3(0,0,0));
            }

        }
        
        //update velocity and position 
        foreach (Boid boid in boids) {
            boid.update_velocity(speed_multiplier * dt);
            //TODO: clamp velocity here
            boid.set_velocity(clamp(boid.get_velocity(), min_speed, max_speed));
            //update position, bounce bird if needed
            boid.update_position(speed_multiplier * dt);
            check_boundary(boid);
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

    void check_boundary(Boid boid) {
        // float error = 0.1f;
        Vector3 position = boid.get_position();
        Vector3 velocity = boid.get_velocity();
        if (position.x >= east_wall || position.x <= west_wall) {
            boid.set_velocity(new Vector3(-velocity.x, velocity.y, velocity.z));
        } 
        if (position.z >= north_wall || position.z <= south_wall) {
            boid.set_velocity(new Vector3(velocity.x, velocity.y, -velocity.z));
        }
    }

    Vector3 clamp(Vector3 vel, float min_speed, float max_speed) {
        float magnitude = Vector3.Magnitude(vel);
        if (magnitude >= min_speed && magnitude <= max_speed) {
            return vel;
        } else if (magnitude < min_speed) {
            return Vector3.Normalize(vel) * min_speed;
        } else if (magnitude > max_speed) {
            return Vector3.Normalize(vel) * max_speed;
        }
        return new Vector3(0,0,0);
    }

    Vector3 find_flocking_force(Boid[] boids, Boid current_boid, float flock_radius) {
        // List<Boid> output = new List<Boid>();
        const float epsilon = 0.01f;
        float weight_total = 0;
        Vector3 numerator = new Vector3(0,0,0);
        foreach (Boid b in boids) {
            if (b == current_boid) continue;
            float distance = Vector3.Distance(b.get_position(), current_boid.get_position());
            
            //calculate flocking force
            if (distance <= flock_radius) {
                // found close enough neighbor
                float weight = 1 / (distance * distance + epsilon);
                numerator += weight * (b.get_position() - current_boid.get_position());
                weight_total += weight;
            }
        }
        if (weight_total == 0) return new Vector3(0,0,0);
        return numerator / weight_total;
    }

    Vector3 find_collision_force(Boid[] boids, Boid current_boid, float collision_radius) {
        const float epsilon = 0.01f;
        Vector3 total = new Vector3(0,0,0);
        foreach (Boid b in boids) {
            if (b == current_boid) continue;
            float collision_distance = Vector3.Distance(current_boid.get_position(), b.get_position());
            if (collision_distance < collision_radius) {
                float weight = 1 / (collision_distance * collision_distance + epsilon);
                total += weight * (current_boid.get_position() - b.get_position());
            }
        }
        return total;
    }

    Vector3 find_velocity_matching_force(Boid[] boids, Boid current_boid, float velocity_matching_force) {
        const float epsilon = 0.01f;
        Vector3 total = new Vector3(0,0,0);
        foreach (Boid b in boids) {
            if (b == current_boid) continue;
            float distance = Vector3.Distance(current_boid.get_position(), b.get_position());
            if (distance < velocity_matching_force) {
                float weight = 1 / (distance * distance + epsilon);
                total += weight * (b.get_velocity() - current_boid.get_velocity());
            }
        }
        return total;
    }

    void scatter(Boid[] boids) {
        foreach (Boid b in boids) {
            //should i give them a new random_velocity??
            Vector3 rand_velocity = new Vector3(random_from(min_vel, max_vel), 0, random_from(min_vel, max_vel));
            Vector3 rand_pos = new Vector3(random_from(min_x_pos, max_x_pos), 0, random_from(min_z_pos, max_z_pos));

            if (leave_trail) {
                b.disable_trail();
                b.enable_trail();
            }
            b.set_position(rand_pos);
        }
    }
}