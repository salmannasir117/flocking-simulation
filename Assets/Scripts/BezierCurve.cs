using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
    Vector3 P1, P2, P3, P4;
    int num_points;     //store n, get points [0, n] inclusive. 
    // Vector3[] points;

    public BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int n) {
        P1 = p1;
        P2 = p2;
        P3 = p3;
        P4 = p4;
        num_points = n; //to account for i = n / num_points = 1
    }

    //weights based on parameter t \in [0,1]
    
    //B1(t) = (1 - t)^3
    float B1(float t) {
        return (1 - t) * (1 - t) * (1 - t);
    } 
    
    //B2(t) = 3t(1 - t)^2
    float B2(float t) {
        return 3 * t * (1 - t) * (1 - t);
    }
    
    //B3(t) = 3t^2 * (1 - t)
    float B3(float t) {
        return 3 * t * t * (1 - t);
    }

    //B4(t) = t^3
    float B4(float t) {
        return t * t * t;
    }
    
    public Vector3 get_point(int n) {
        //generate at runtime
        float t = n / (float) (num_points - 1);
        Vector3 curr = new Vector3();
        curr = B1(t) * P1 + B2(t) * P2 + B3(t) * P3 + B4(t) * P4;
        return curr;
    }

    public int get_num_points() {
        return num_points;
    }
}
