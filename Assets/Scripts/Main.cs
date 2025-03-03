using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    
    public bool flock_centering, velocity_matching, collision_avoidance, wandering, leave_trail;
    List<Boid> boids;
    public int num_boids = 30;
    const float dt = 0.005f;
    public float speed_multiplier = 1;
    const int MAX_BOIDS = 1000;

    const float west_wall = -52.0f, east_wall = 52.0f, north_wall = 29.0f, south_wall = -29.0f;
    const float min_speed = 0.15f, max_speed = 1.5f;
    const float flock_radius = 2.0f, collision_radius = 1.5f, velocity_matching_radius = 1.2f;
    
    //quick and dirty to keep these public for scatter function to see, for initial random conditions
    float max_vel = 1.0f, min_vel = -1.0f;
    float max_z_pos = north_wall - 1.0f, min_z_pos = south_wall + 1.0f;
    float max_x_pos = east_wall - 1.0f, min_x_pos = west_wall + 1.0f;
        
    void Start()
    {
        // num_boids = 30;
        boids = new List<Boid>(num_boids);
        for (int i = 0; i < num_boids; i++) {
            Vector3 rand_velocity = new Vector3(random_from(min_vel, max_vel), 0, random_from(min_vel, max_vel));
            Vector3 rand_pos = new Vector3(random_from(min_x_pos, max_x_pos), 0, random_from(min_z_pos, max_z_pos));
            // boids[i] = new Boid(rand_pos, rand_velocity);
            boids.Insert(i, new Boid(rand_pos, rand_velocity));
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        //scatter on spacebar press
        if (Input.GetKeyDown(KeyCode.Space)) {
            scatter(boids);
        }
        
        //user wants less boids -> delete the game object, remove the boid from the list
        if (num_boids < boids.Count && num_boids >= 0) {
            int bc = boids.Count;
            for (int i = 0; i < bc - num_boids; i++) {
                Boid b = boids[0];
                b.get_game_object().GetComponent<Renderer>().material.color = Color.black;
                Destroy(b.get_game_object());
                boids.Remove(b);
            }
        } else if (num_boids > boids.Count && num_boids <= MAX_BOIDS) {     //user wants more boids -> generate more, up to max number
            int bc = boids.Count;
            for (int i = 0; i < num_boids - bc; i++) {
                Vector3 rand_velocity = new Vector3(random_from(min_vel, max_vel), 0, random_from(min_vel, max_vel));
                Vector3 rand_pos = new Vector3(random_from(min_x_pos, max_x_pos), 0, random_from(min_z_pos, max_z_pos));
                boids.Add(new Boid(rand_pos, rand_velocity));
            }
        }

        //calculate forces/trail
        foreach (Boid boid in boids) {
                
            //generate trail
            if (leave_trail) {
                boid.enable_trail();
            } else {
                boid.disable_trail();
            }

            //interate, get each individual force.
            //if (not enabled), set force to zero
            //wander force 
            if (wandering) {
                Vector3 wander_force = new Vector3(random_from(-1, 1), 0, random_from(-1, 1));
                boid.set_wander_force(wander_force);
            } else {
                boid.set_wander_force(new Vector3(0,0,0));
            }

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
        
        //update + clamp velocity and position
        foreach (Boid boid in boids) {
            boid.update_velocity(speed_multiplier * dt * Time.deltaTime);
            boid.set_velocity(clamp(boid.get_velocity(), min_speed, max_speed));
            //update position, bounce bird if needed
            boid.update_position(speed_multiplier * dt * Time.deltaTime);
            check_boundary(boid);
        }
        
        //draw 
        foreach (Boid boid in boids) {
            if (boid == null) continue;
            boid.draw();
        } 
    }

    //generate random float from interval [min, max]
    float random_from(float min, float max) {
        return Random.value * (max - min) + min;
    }

    //have boids bounce off walls if hitting them
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

    //clamp speed of void so never too fast or slow
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

    Vector3 find_flocking_force(List<Boid> boids, Boid current_boid, float flock_radius) {
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

    Vector3 find_collision_force(List<Boid> boids, Boid current_boid, float collision_radius) {
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

    Vector3 find_velocity_matching_force(List<Boid> boids, Boid current_boid, float velocity_matching_force) {
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

    //scatter all boids to random positions with random velocities
    void scatter(List<Boid> boids) {
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