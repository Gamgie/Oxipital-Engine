using System.Linq;
using UnityEngine;

// Each Gaussian splat is controlled by its own GaussianSplatVisibility component
// (living on the same GameObject as its GaussianSplatRenderer) so OSCQuery/Chataigne
// can address it directly - OSCQuery only exposes flat public fields per component,
// it cannot see a list held on this manager. This manager is just a convenience
// lookup for controlling splats by name from other Unity scripts.
public class GaussianManager : MonoBehaviour
{
    public GaussianSplatVisibility[] Splats => GetComponentsInChildren<GaussianSplatVisibility>(true);

    public void SetVisibility(string splatName, float target)
    {
        GaussianSplatVisibility splat = Find(splatName);
        if (splat != null)
            splat.visibility = Mathf.Clamp01(target);
    }

    public void FadeIn(string splatName) => SetVisibility(splatName, 1f);
    public void FadeOut(string splatName) => SetVisibility(splatName, 0f);

    GaussianSplatVisibility Find(string splatName)
    {
        return Splats.FirstOrDefault(s => s.gameObject.name == splatName);
    }
}
