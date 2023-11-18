using System.Diagnostics;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    public int Iterations = 1000;

    Vector3 vecA = new(1.253f, 345.23443f, 85723.9375f);
    Vector3 vecB = new(235.253f, 15.1243f, 23.7425f);

    [ContextMenu("Run Test")]
    public void RunTest()
    {
        int count = 0;
        Stopwatch sw = new();
        sw.Start();
        for (int i = 0; i < Iterations; i += 1)
        {
            count += 1;
            // var resultA = Vector3.Dot(vecA, vecB);
            var resultA = Vector3.Dot(vecA, vecB) * Mathf.Cos(3.14f) + Mathf.Sign(3.14f * 2f);
            var resultB = Vector3.Dot(vecA, vecB) * Mathf.Cos(2.14f) + Mathf.Sign(2.14f * 2f);
            var resultC = Vector3.Dot(vecA, vecB) * Mathf.Cos(1.14f) + Mathf.Sign(1.14f * 2f);
            var rotatedVector = Quaternion.Euler(23f, 14.537f, 66.11562f) * vecA;
            var finalResult = Mathf.Pow(resultA, 2) / Mathf.Sqrt(resultA);
        }

        sw.Stop();
        UnityEngine.Debug.Log($"Linear time: {sw.Elapsed}, ticks: {sw.ElapsedTicks}");

        
        count = 0;
        sw.Restart();
        for (int i = 0; i < Iterations; i += 1)
        {
            for (int j = 0; j < Iterations; j += 1)
            {
                count += 1;
                // var resultA = Vector3.Dot(vecA, vecB);
                var resultA = Vector3.Dot(vecA, vecB) * Mathf.Cos(3.14f) + Mathf.Sign(3.14f * 2f);
                var resultB = Vector3.Dot(vecA, vecB) * Mathf.Cos(2.14f) + Mathf.Sign(2.14f * 2f);
                var resultC = Vector3.Dot(vecA, vecB) * Mathf.Cos(1.14f) + Mathf.Sign(1.14f * 2f);
                var rotatedVector = Quaternion.Euler(23f, 14.537f, 66.11562f) * vecA;
                var finalResult = Mathf.Pow(resultA, 2) / Mathf.Sqrt(resultA);
            }
        }

        sw.Stop();
        UnityEngine.Debug.Log($"Exponential time: {sw.Elapsed}, ticks: {sw.ElapsedTicks}");
    }
}
