using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NBodyProblemPattern : DancePattern
{
    public float G = 1f; // Gravitational constant
    public float bodyMass = 1;

    public List<Vector3> prevPositions;

    // Start is called before the first frame update
    override protected List<Vector3> getPatternPositions(DancerGroup group)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < group.dancers.Count; i++)
        {
            if (i >= prevPositions.Count)
            {
                prevPositions.Add(Vector3.zero);
            }

            if (prevPositions[i] == Vector3.zero)
            {
                prevPositions[i] = Random.insideUnitSphere * group.patternSize;
            }
        }

        for (int i = 0; i < group.dancers.Count; i++)
        {
            Vector3 acceleration = Vector3.zero;
            for (int j = 0; j < group.dancers.Count; j++)
            {
                if (i == j) continue;
                acceleration += CalculateGravitationalAcceleration(prevPositions[i], prevPositions[j]);
            }

            group.dancers[i].velocity += acceleration * Time.deltaTime * group.patternSpeed;
            positions.Add(prevPositions[i] + group.dancers[i].velocity * Time.deltaTime * group.patternSpeed);

            prevPositions[i] = positions[i];
        }



        return positions;
    }

    private Vector3 CalculateGravitationalAcceleration(Vector3 pos1, Vector3 pos2)
    {
        Vector3 direction = pos2 - pos1;
        float distance = direction.magnitude;
        return G * bodyMass / (distance * distance * distance) * direction;
    }
}
