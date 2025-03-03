using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPatch
{
    BezierCurve G1, G2, G3, G4;
    int num_points_per_size;
    int num_points;

    Vector3 [] points;
    int [] triangles;
    //should i not store the points and get them at runtime? I htink that would be more efficient/wanted

    public BezierPatch(Vector3 [,] ctrl_pts, int n) {       //n is number of points per dimension
        num_points_per_size = n;
        num_points = n * n;
        points = new Vector3[num_points];
        triangles = new int[n * n * 6];
        G1 = new BezierCurve(ctrl_pts[0,0], ctrl_pts[0, 1], ctrl_pts[0, 2], ctrl_pts[0, 3], n);
        G2 = new BezierCurve(ctrl_pts[1,0], ctrl_pts[1, 1], ctrl_pts[1, 2], ctrl_pts[1, 3], n);
        G3 = new BezierCurve(ctrl_pts[2,0], ctrl_pts[2, 1], ctrl_pts[2, 2], ctrl_pts[2, 3], n);
        G4 = new BezierCurve(ctrl_pts[3,0], ctrl_pts[3, 1], ctrl_pts[3, 2], ctrl_pts[3, 3], n);

        for (int j = 0; j < n; j++) {
            // float t = j / (float) (n - 1);
            BezierCurve curr = new BezierCurve(G1.get_point(j), G2.get_point(j), G3.get_point(j), G4.get_point(j), n);
            for (int i = 0; i < curr.get_num_points(); i++) {
                points[j * n + i] = curr.get_point(i);
            }
        }
    }

    public Vector3[] get_points() {
        return points;
    }

    public int get_num_points() {
        return num_points;
    }

    void generate_triangles() {
        int x_size = num_points_per_size;
        int y_size = num_points_per_size;
        int[] tris = new int[x_size * y_size * 6];  //2 triangles per square, 3 points per triangle

        int triangle_index = 0, vertex_index = 0;
		for (int y = 0; y < y_size - 1; y++) {
			for (int x = 0; x < x_size - 1; x++) {
                //works for floor, bottom, and left
                //triangle 1:

                tris[triangle_index] = vertex_index;    //bottom left
                tris[triangle_index + 1] = vertex_index + 1;   //top left 
                tris[triangle_index + 2] = vertex_index + x_size;    //bottom right

                //triangle 2:
				tris[triangle_index + 3] = vertex_index + y_size;    //bottom right
				tris[triangle_index + 4] = vertex_index + 1;   //top left
				tris[triangle_index + 5] = vertex_index + y_size + 1;   //top right

                triangle_index += 6;    //added 6 verts
                vertex_index++;         //move to right
			}
            vertex_index++;             //move to next row
		}
        triangles = tris;
    }

    public Mesh get_mesh() {
        Mesh m = new Mesh();
        m.vertices = points;
        generate_triangles();
        m.triangles = triangles;
        m.RecalculateNormals();
        return m;
    }

    public GameObject get_game_object(String name, Color color) {
        Mesh m = get_mesh();
        GameObject output = new GameObject(name);
        output.AddComponent<MeshFilter>();
        output.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        output.GetComponent<MeshFilter>().mesh = m;

        // change the texture of the object
        Renderer rend = output.GetComponent<Renderer>();
        rend.material.color = color;
        return output;
    }
}
