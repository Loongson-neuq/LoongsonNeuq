namespace LoongsonNeuq.ListFormatter;

public interface IChecker
{
    public bool CheckOrNormalize(ref ListRoot root);
}