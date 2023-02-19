using System;

namespace Letteral.Rabbitmq.Contracts;

public readonly struct BindingType : IEquatable<BindingType>
{
    public static BindingType Fanout = new ("fanout");
    public static BindingType Header = new ("header");
    public static BindingType Topic = new ("topic");
    public static BindingType Direct = new ("direct");
        
    private readonly string _value;
    private BindingType(string value)
    {
        _value = value;
    }
        
    public override string ToString()
    {
        return _value;
    }
    
    public bool Equals(BindingType other)
    {
        return _value == other._value;
    }
    
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj.GetType() == this.GetType() && Equals((BindingType)obj);
    }
    
    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }
}