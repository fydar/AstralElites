using System;

[Serializable]
public class EffectFader
{
    public LoopGroup Audio;

    private readonly IInterpolator Interpolator;

    public float Value => Interpolator.Value;

    public float TargetValue
    {
        set => Interpolator.TargetValue = value;
    }

    public EffectFader(IInterpolator interpolator)
    {
        Interpolator = interpolator;
    }

    public void Update(float deltaTime)
    {
        Interpolator.Update(deltaTime);
    }
}
