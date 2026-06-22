using System;

public class EventFieldChainHandler<T, TChild> : IEventFieldHandler
{
    public EventField<T> SourceField;
    public EventField<TChild> TargetField;
    public Func<T, EventField<TChild>> Chain;

    private EventField<TChild> ChainedField;

    public EventFieldChainHandler(EventField<T> source, EventField<TChild> target, Func<T, EventField<TChild>> chain)
    {
        SourceField = source;
        TargetField = target;
        Chain = chain;

        ChainedField = Chain(SourceField.Value);
    }

    public void OnBeforeChanged()
    {
        if (ChainedField == null)
        {
            return;
        }

        ChainedField.handlers[this].Clear();
    }

    public void OnAfterChanged()
    {
        ChainedField = Chain(SourceField.Value);
        if (ChainedField == null)
        {
            TargetField.Value = default;
            return;
        }

        ChainedField.handlers[this] += new EventFieldMirrorHandler<TChild>(ChainedField, TargetField);
        TargetField.Value = ChainedField.Value;
    }

    public void Dispose()
    {
        SourceField.handlers[TargetField].Clear();

        if (ChainedField == null)
        {
            return;
        }

        ChainedField.handlers[this].Clear();
    }
}
