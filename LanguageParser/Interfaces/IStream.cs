namespace LanguageParser.Interfaces;

public interface IStream<out T>
{
    public int Index { get; }
    
    public T Current { get; }
    public T Next { get; }
    
    public bool CanAdvance { get; }
    public void Advance();
}