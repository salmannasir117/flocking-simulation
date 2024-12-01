using UnityEngine;

public class Boid
{
    Vector3 position;
    Vector3 velocity;

    GameObject game_object;
    const float wander_weight = 1.6f, flocking_weight = 2.0f, collision_weight = 2.15f, velocity_matching_weight = 1.8f;
    Vector3 wander_force, flocking_force, collision_force, velocity_matching_force;

    public Boid(Vector3 pos, Vector3 vel) {
        position = pos;
        velocity = vel;
        wander_force = new Vector3(0,0,0);
        flocking_force = new Vector3(0,0,0);
        collision_force = new Vector3(0,0,0);
        Mesh mesh = make_mesh();
        game_object = make_game_object(mesh);

        game_object.transform.position = pos;
        Vector3 dir = Vector3.Normalize(velocity);
        // Vector3 relativePos = target.position - transform.position;

        // the second argument, upwards, defaults to Vector3.up
        Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
        game_object.transform.rotation = rotation;


    }

    public void set_position(Vector3 pos) {
        position = pos;
    }
    public Vector3 get_position() {
        return position;
    }

    public void set_velocity(Vector3 vel) {
        //TODO: ensure that vel is not above max or below min
        velocity = vel;
    }
    public Vector3 get_velocity() {
        return velocity;
    }

    public Vector3 get_direction() {
        return Vector3.Normalize(velocity);
    }

    public void set_wander_force(Vector3 v) {
        wander_force = v;
    }

    public void set_flocking_force(Vector3 v) {
        flocking_force = v;
    }

    public void set_collision_force(Vector3 v) {
        collision_force = v;
    }

    public void set_velocity_matching_force(Vector3 v) {
        velocity_matching_force = v;
    }
    Vector3 get_total_force() {
        return wander_weight * wander_force 
            + flocking_weight * flocking_force 
            + collision_weight * collision_force
            + velocity_matching_weight * velocity_matching_force;
    }
    public void update_position(float dt) {
        position = position + velocity * dt;
    }
    
    public void update_velocity(float dt) {
        Vector3 total_force = get_total_force();
        velocity = velocity + dt * total_force;
    }

    public void enable_trail() {
        TrailRenderer tr = game_object.GetComponent<TrailRenderer>();
        tr.enabled = true;
    }

    public void disable_trail() {
        TrailRenderer tr = game_object.GetComponent<TrailRenderer>();
        tr.Clear();
        tr.enabled = false;
    }
    
    //set gameobject position and rotation based on position and velocity
    public void draw() {
        Vector3 dir = Vector3.Normalize(velocity);
       
        // the second argument, upwards, defaults to Vector3.up
        Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
        game_object.transform.rotation = rotation;

        game_object.transform.position = position;
    }
    
    Mesh make_mesh() {
        float body_width = 0.5f;
        float body_height = 1.0f;
        float head_width = 1.0f;
        float head_height = 1.0f;

        float total_height = body_height + head_height;

        Mesh m = new Mesh();

        Vector3[] verts = { 
            // new Vector3(-body_width / 2, 0, -body_height / 2),
            // new Vector3(-body_width / 2, 0, body_height / 2),
            // new Vector3(body_width / 2, 0, body_height / 2),
            // new Vector3(body_width / 2, 0, -body_height / 2),

            // new Vector3(-head_width / 2, 0, + body_height / 2),
            // new Vector3(0, 0, head_height + body_height / 2),
            // new Vector3(head_width / 2, 0, + body_height / 2),

            new Vector3(-body_width / 2, 0, -total_height / 2),
            new Vector3(-body_width / 2, 0, -total_height / 2 + body_height),
            new Vector3(body_width / 2, 0, - total_height / 2 + body_height),
            new Vector3(body_width / 2, 0, -total_height / 2),

            new Vector3(-head_width / 2, 0, -total_height / 2 + body_height),
            new Vector3(0, 0, total_height / 2),
            new Vector3(head_width / 2, 0, -total_height / 2 + body_height),
        };

        int[] tris = {
            0, 1, 2, 0, 2, 3,
            4, 5, 6,
        };

        m.vertices = verts;
        m.triangles = tris;
        m.RecalculateNormals();

        return m;
    }

    GameObject make_game_object(Mesh mesh) {
        GameObject output = new GameObject("temp name");
        output.AddComponent<MeshFilter>();
        output.AddComponent<MeshRenderer>();
        output.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = output.GetComponent<Renderer>();
        rend.material.color = Color.white;

        output.AddComponent<TrailRenderer>();
        TrailRenderer tr = output.GetComponent<TrailRenderer>();
        tr.startWidth = 0.5f;
        tr.endWidth = 0.5f;
        tr.time = 2.0f;
        return output;
    }

    public GameObject get_game_object() {
        return game_object;
    }
}
