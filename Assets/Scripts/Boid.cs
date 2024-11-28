using UnityEngine;

public class Boid
{
    Vector3 position;
    Vector3 velocity;

    GameObject game_object;
    const float wander_weight = 1.6f;
    Vector3 wander_force;

    public Boid(Vector3 pos, Vector3 vel) {
        position = pos;
        velocity = vel;
        wander_force = new Vector3(0,0,0);
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

    public void set_wander_force(Vector2 v) {
        wander_force = v;
    }

    Vector3 get_total_force() {
        return wander_weight * wander_force;
    }
    public void update_position(float dt) {
        position = position + velocity * dt;
    }
    
    public void update_velocity(float dt) {
        Vector3 total_force = get_total_force();
        velocity = velocity + dt * total_force;
    }

    //set gameobject position and rotation based on position and velocity
    public void draw() {
        Vector3 dir = Vector3.Normalize(velocity);
        // Vector3 relativePos = target.position - transform.position;

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
        return output;
    }
}
