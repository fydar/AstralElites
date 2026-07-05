public interface IInterpolator
{
    public float Value { get; set; }
    public float TargetValue { set; }
    public bool Sleeping { get; }

    public void Update(float deltaTime);
}
