
using UnityEngine;

public class Distribution : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ox: enemies defeated / max enemies|Oy: spawn chance")]
    private AnimationCurve spawnProbability;
    [SerializeField]
    [Tooltip("Overrides the default rotation if needed")]
    private Vector3 rotationOverride;
    [SerializeField]
    [Tooltip("Overrides the default position if needed")]
    private Vector3 positionOverride;

    public bool ProbabilityCheck(float currentEnemyPercentage, float chance)
    {
        // Rolls the dice for base spawn chance
        var dice = Random.Range(0f, 1f);
        if (dice < chance)
        {
            // Roll for distribution chance
            // 'Time' is the Horizontal Axis of the animation curve. In this case, 'Time' is the current ratio between enemies defeated and max enemies
            var distribution = Random.Range(0f, 1f);
            if (distribution < spawnProbability.Evaluate(currentEnemyPercentage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public Quaternion OverrideRotation()
    {
        if (rotationOverride != Vector3.zero)
            return Quaternion.Euler(rotationOverride);
        else
            return Quaternion.identity;
    }
    public Vector3 PositionBuffer(Vector3 origin)
    {
        if (positionOverride != Vector3.zero)
            return origin + positionOverride;
        else
            return origin;
    }
}
