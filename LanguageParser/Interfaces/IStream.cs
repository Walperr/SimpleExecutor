namespace LanguageParser.Interfaces;

public interface IStream<out T>
{
    public int Index { get; }
    
    public T Current { get; }
    public T Next { get; }
    
    public T? Previous { get; }
    
    public bool CanAdvance { get; }
    public void Advance();
    public bool CanRecede { get; }
    public void Recede();
}