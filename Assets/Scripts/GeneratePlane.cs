using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class GeneratePlane 
{
    // Start is called before the first frame update
    public int seed = 0;

    int num_wings = 3;
    enum wing_type {
        ORIGINAL = 0,
        FANCY = 1,
        CURVY = 2,
    }
    int num_tails = 3;
    enum tail_type {
        ORIGINAL = 0,
        DOLPHIN = 1,
        CAPE = 2,
    }

    Color[] plane_colors = 
    {
        //plane grey
        new Color(196,203,212) / 255.0f,
        //spirit yellow
        new Color(255, 236, 0) / 255.0f,
        //jet blue
        new Color(0, 56, 118) / 255.0f,
        //La compagnie light blue
        new Color(148, 196, 228) / 255.0f,
        //macy's red (they have floats/thanksgiving day parade)
        new Color(234, 0, 0) / 255.0f,
    };

    // to allow our computers to not blow up.
    // this is the number of points per bezier curve on a bezier curve.
    // so, if HULL_RESOLUTION = 10, then each batch in the hull has 10 * 10 = 100 verticies. 
    // HULL is considered 3 pieces: top, bottom, back
    // WING is 2 pieces: left and right wing. 
    //10 -> performance mode
    //20 -> quality mode
    public int resolution = 20;
    int WING_RESOLUTION;
    int HULL_RESOLUTION;
    int TAIL_RESOLUTION;
    const float TAIL_MIN_SCALE = 0.9f;
    const float TAIL_MAX_SCALE = 1.1f;
    
    const float WING_MIN_SCALE = 0.75f;
    const float WING_MAX_SCALE = 1.25f;  

    public enum flight_pattern {
        PARKED,
        FLYING,
    }  

    public flight_pattern f_pattern = flight_pattern.PARKED;
    void Start()
    {
        Random.InitState(seed);
        float plane_space = 12.5f;      //for when parked
        TAIL_RESOLUTION = HULL_RESOLUTION = WING_RESOLUTION = resolution;

        HashSet<Vector3> plane_pos = new HashSet<Vector3>();
        //for each plane:
        //generate random wing type
        //generate random tail type
        //generate color
        //generate wing scale
        //generate tail scale
        //generate plane
        for (int i = 0; i < 5; i++) {
            int plane_number = i + 1;
            //select wing type & scale
            int wing_number = Random.Range(0, num_wings);
            wing_type wing = (wing_type) wing_number;
            float wing_scale = Random.value * (WING_MAX_SCALE - WING_MIN_SCALE) + WING_MIN_SCALE;


            //select tail type & scale
            int tail_number = Random.Range(0, num_tails);
            tail_type tail_t = (tail_type) tail_number;
            float tail_scale = Random.value * (TAIL_MAX_SCALE - TAIL_MIN_SCALE) + TAIL_MIN_SCALE;


            // Select random color
            Color selected_color = plane_colors[Random.Range(0, plane_colors.Length)];

            GameObject parent = new GameObject("plane " + plane_number);
            GameObject top_hull  = generate_top_hull_go(parent, selected_color);
            GameObject bottom_hull = generate_bottom_hull_go(parent, selected_color);
            GameObject back_hull = generate_back_hull_go(parent, selected_color);
            GameObject left_wing = generate_left_wing(parent, selected_color, wing, new Vector3(1, 1.2f, 1.2f));
            GameObject right_wing = generate_right_wing(parent, selected_color, wing, new Vector3(1, 1.2f, 1.2f));
            GameObject tail = generate_tail(parent, selected_color, tail_t, new Vector3(1, 0.8f, 1.2f));

            if (f_pattern == flight_pattern.PARKED) {
                parent.transform.Translate(new Vector3(i * plane_space, 0, 0));
            } else if (f_pattern == flight_pattern.FLYING) {
                
                //generate position using dart throwing 
                float bounds = 25;      //sample in a cube that is centered around (0,0,0) and has side length 2 * bounds
                float min_dist = 10;    //how far apart each plane should be on minimum
                bool valid = false;     //marker to know when we have valid posiiton
                Vector3 translate = new Vector3(0,0,0);
                while (!valid) {
                    translate = new Vector3(Random.value * bounds * 2 - bounds, Random.value * bounds* 2 - bounds, Random.value * bounds * 2 - bounds);
                    valid = true;
                    foreach (Vector3 pos in plane_pos) {                        //check distance with each generated plane
                        if (Vector3.Distance(translate, pos) < min_dist) {      //if too close, have to generate new point
                            valid = false;
                        }
                    }
                }
                parent.transform.Translate(translate);                          //move the plane

                //generate rotations to make fly
                float max_x = 20;
                float min_x = -20;
                float max_y = 20;
                float min_y = -20;
                float max_z = 45;
                float min_z = -45;

                float rotate_x = Random.value * (max_x - min_x) + min_x;
                float rotate_y = Random.value * (max_y - min_y) + min_y;
                float rotate_z = Random.value * (max_z - min_z) + min_z;

                parent.transform.Rotate(new Vector3(rotate_x, rotate_y, rotate_z));     //rotate the plane randomly
                plane_pos.Add(translate);                                               //add plane to checklist
            }
            
            //test transformations. 
            //https://docs.unity3d.com/ScriptReference/Transform.html
            //localScale, Translate, Rotate (relativeTo = Space.self | Space.world, RotateAround
            // parent.transform.Translate(new Vector3(1, 2, 0));
            // parent.transform.localScale = new Vector3(2, 1, 1);
        }
    }

    Vector3 [,] get_top_hull_points() {
        Vector3 [,] top_hull_points = {
            {new Vector3(0,0,0), new Vector3(1.5f,0f,-2f), new Vector3(1.5f,0,-2f), new Vector3(3,0,0),},
                                //make below (1,2,1) for uniform shape
            {new Vector3(0,0,0.1f), new Vector3(1,5,0.1f), new Vector3(2,5,0.1f), new Vector3(3,0,0.1f),},
            {new Vector3(0,0,5), new Vector3(1,2,5), new Vector3(2,2,5), new Vector3(3,0,5),},
            {new Vector3(0,0,7), new Vector3(1,2,7), new Vector3(2,2,7), new Vector3(3,0,7),},
        };
        return top_hull_points;
    }

    GameObject generate_top_hull_go(GameObject parent, Color color) {
        Vector3[,] top_hull_points = get_top_hull_points();
        BezierPatch top_hull_patch = new BezierPatch(top_hull_points, HULL_RESOLUTION);
        GameObject top_hull_go = top_hull_patch.get_game_object("top hull", color);
        top_hull_go.transform.parent = parent.transform;
        return top_hull_go;
    }
    GameObject generate_bottom_hull_go(GameObject parent, Color color) {
        GameObject bottom_hull = generate_top_hull_go(parent, color);
        bottom_hull.name = "bottom hull";
        bottom_hull.transform.Translate(new Vector3(3, 0, 0));
        bottom_hull.transform.Rotate(new Vector3(0, 0, 180), Space.Self);
        return bottom_hull;

    }

    GameObject generate_back_hull_go(GameObject parent, Color color) {
        Vector3[,] back_hull_points = {
            {new Vector3(0,0,7), new Vector3(1,2,7), new Vector3(2,2,7), new Vector3(3,0,7),},
            {new Vector3(0,-0,7), new Vector3(1,0,7), new Vector3(2,0,7), new Vector3(3,0,7),},
            {new Vector3(0,-0,7), new Vector3(1,0,7), new Vector3(2,0,7), new Vector3(3,0,7),},
            {new Vector3(0,-0,7), new Vector3(1,-2,7), new Vector3(2,-2,7), new Vector3(3,-0,7),},
        };
        BezierPatch back_hull_patch = new BezierPatch(back_hull_points, HULL_RESOLUTION);
        GameObject back_hull_go = back_hull_patch.get_game_object("back hull", color);
        back_hull_go.transform.parent = parent.transform;
        return back_hull_go;
    }

    Vector3[,] get_original_wing_points() {
        return new Vector3[,]{
            {new Vector3(0,0,2), new Vector3(0,0,2.5f), new Vector3(0,0,3), new Vector3(0,0,5),},
            {new Vector3(-1,0,2.5f), new Vector3(-1,0,3f), new Vector3(-1,0,3.5f), new Vector3(-1,0,5),},
            {new Vector3(-2,0,3f), new Vector3(-2,0,3.5f), new Vector3(-2,0,4f), new Vector3(-2,0,5),},
            {new Vector3(-3,0,3.5f), new Vector3(-3,0,4f), new Vector3(-3,0,4.5f), new Vector3(-3,0,5),},
        };
    }

    Vector3[,] get_fancy_wing_points() {
        return new Vector3[,]{
            {new Vector3(0,0,2), new Vector3(0,0,2.5f), new Vector3(0,0,3f), new Vector3(0,0,3.5f),},
            {new Vector3(-1,0,3.5f), new Vector3(-1,0,4f), new Vector3(-1,0,4.5f), new Vector3(-1,0,4.35f),},

            {new Vector3(-2.8f,0,4f), new Vector3(-2.8f,0,4.5f), new Vector3(-2.8f,0,5f), new Vector3(-2.8f,0,5),},

            {new Vector3(-3,1f,4.5f), new Vector3(-3,1f,5f), new Vector3(-3,1f,5.5f), new Vector3(-3,1f,6),},
        };
    }

    Vector3[,] get_curvy_wing_points() {
        float height = 3;
        return new Vector3[,]{
            {new Vector3(0, -0.5f,2), new Vector3(0,0,2.75f), new Vector3(0,0,3.5f), new Vector3(0,0,4.25f),},
            {new Vector3(-1, height -0.5f, 2.75f), new Vector3(-1, height, 3.5f), new Vector3(-1, height, 4.25f), new Vector3(-1, height, 5f),},

            {new Vector3(-2.8f, -height -0.5f, 3.5f), new Vector3(-2.8f, -height, 4.25f), new Vector3(-2.8f, -height, 5.0f), new Vector3(-2.8f, -height, 5.75f),},

            {new Vector3(-4, height / 2 -0.5f, 4.5f), new Vector3(-4, height / 2, 5.25f), new Vector3(-4, height / 2,5.75f), new Vector3(-4,height / 2,6.25f),},
        };
    }
    GameObject generate_left_wing(GameObject parent, Color color, wing_type wing, Vector3 scale) {
        Vector3 [,] points; 
        switch (wing) {
            case wing_type.CURVY: {
                points = get_curvy_wing_points();
                break;
            }
            case wing_type.FANCY: {
                points = get_fancy_wing_points();
                break;
            }
            case wing_type.ORIGINAL:
            default: {
                points = get_original_wing_points();
                break;
            }
        }

        //ignore the first row as that is the curve that attaches the components.
        for (int i = 1; i < points.GetLength(0); i++) {
            for (int j = 0; j < points.GetLength(1); j++) {
                points[i,j].Scale(scale);
            }
        }
        
        BezierPatch bp = new BezierPatch(points, WING_RESOLUTION);
        GameObject go = bp.get_game_object("left wing", color);
        go.transform.parent = parent.transform;
        return go;
    }
    
    GameObject generate_right_wing(GameObject parent, Color color, wing_type wing, Vector3 scale) {
        // Vector3 rotate;
        Vector3 [,] points; 
        switch (wing) {
             case wing_type.CURVY: {
                points = get_curvy_wing_points();
                break;
            }
            case wing_type.FANCY: {
                points = get_fancy_wing_points();
                break;
            }
            case wing_type.ORIGINAL:
            default: {
                points = get_original_wing_points();
                break;
            }
        }

        //ignore the first row as that is the curve that attaches the components.
        for (int i = 1; i < points.GetLength(0); i++) {
            for (int j = 0; j < points.GetLength(1); j++) {
                points[i,j].Scale(scale);
            }
        }

        reverse_points(points);
        flip_heights(points);   //for when i rotate about the z axis
        BezierPatch bp = new BezierPatch(points, WING_RESOLUTION);
        GameObject go = bp.get_game_object("right wing", color);
        go.transform.Translate(new Vector3(3, 0, 0));
        go.transform.Rotate(new Vector3(0, 0, 180));
        go.transform.parent = parent.transform;
        return go;

    }

    Vector3 [,] get_original_tail_points() {
        Vector3 [,] points = {
            {new Vector3(0,0,7), new Vector3(1,2,7), new Vector3(2,2,7), new Vector3(3,0,7),},
            {new Vector3(0,1,7.5f), new Vector3(1,2,7.5f), new Vector3(2,2,7.5f), new Vector3(3,1,7.5f),},
            {new Vector3(0,1.2f,8f), new Vector3(1,2,8f), new Vector3(2,2,8f), new Vector3(3,1.2f,8f),},
            {new Vector3(0,1.4f,8.5f), new Vector3(1,2,8.5f), new Vector3(2,2,8.5f), new Vector3(3,1.4f,8.5f),},
        };
        return points;
    }

    Vector3 [,] get_dolphin_tail_points() {
        Vector3 [,] points = {
            {new Vector3(0,0,7), new Vector3(1,2,7), new Vector3(2,2,7), new Vector3(3,0,7),},
            {new Vector3(0.5f,1,7.5f), new Vector3(1,2.25f,7.5f), new Vector3(2,2.25f,7.5f), new Vector3(2.5f,1,7.5f),},
            {new Vector3(1.0f,1.2f,8f), new Vector3(1,2.25f,8f), new Vector3(2,2.25f,8f), new Vector3(2.0f,1.2f,8f),},
            {new Vector3(1.5f,1.4f,8.5f), new Vector3(1,2,8.5f), new Vector3(2,2,8.5f), new Vector3(1.5f,1.4f,8.5f),},
        };
        return points;
    }

    Vector3 [,] get_cape_tail_points() {
        Vector3 [,] points = {
            {new Vector3(0,0,7), new Vector3(1,2,7), new Vector3(2,2,7), new Vector3(3,0,7),},
            {new Vector3(-0.5f,0,7.5f), new Vector3(0.4f,2f,7.5f), new Vector3(2.6f,2f,7.5f), new Vector3(3.5f,0f,7.5f),},
            {new Vector3(-1.0f,0.25f,8f), new Vector3(0.4f,2f,8f), new Vector3(2.6f,2f,8f), new Vector3(4.0f,0.25f,8f),},
            {new Vector3(-1.5f,0.5f,8.5f), new Vector3(1,2,8.5f), new Vector3(2,2,8.5f), new Vector3(4.5f,0.5f,8.5f),},
        };
        return points;
    }
    GameObject generate_tail(GameObject parent, Color color, tail_type tail, Vector3 scale) {
        Vector3 [,] points; 
        switch (tail) {
            case tail_type.CAPE: {
                points = get_cape_tail_points();
                break;
            }
            case tail_type.DOLPHIN: {
                points = get_dolphin_tail_points();
                break;
            }
            case tail_type.ORIGINAL:
            default: {
                points = get_original_tail_points();
                break;
            }
        }
        //ignore the first row as that is the curve that attaches the components.
        for (int i = 1; i < points.GetLength(0); i++) {
            for (int j = 0; j < points.GetLength(1); j++) {
                points[i,j].Scale(scale);
            }
        }
        BezierPatch bp = new BezierPatch(points, TAIL_RESOLUTION);
        GameObject go = bp.get_game_object("tail", color);
        go.transform.parent = parent.transform;
        return go;
    }

    // reverse reach row of a 2x2 matrix in place
    void reverse_points(Vector3[,] points) {
        //reverse each row of points.
        for (int i = 0; i < points.GetLength(0); i++) {
            for (int j = 0; j < points.GetLength(1) / 2; j++) {
                Vector3 temp = points[i,j];
                points[i, j] = points[i, points.GetLength(1) - j - 1];
                points[i, points.GetLength(1) - j - 1] = temp;
            }
        }
    }

    // take an array of points and flip the y coordinate
    void flip_heights(Vector3[,] points) {
        for (int i = 0; i < points.GetLength(0); i++) {
            for (int j = 0; j < points.GetLength(1); j++) {
                points[i,j].y *= -1;
            }
        }
    }

    // void OnDrawGizmosSelected() {
    //     Vector3[,] points = get_cape_tail_points();
    //     foreach (Vector3 point in points) {
    //         Gizmos.DrawSphere(point, 0.1f);
    //     }
    // }
}
