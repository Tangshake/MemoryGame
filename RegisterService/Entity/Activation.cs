namespace RegisterService.Entity;

public class Activation
{
    public int PlayerId { get; set; }
    public DateTime ActivationStart { get; set; }
    public DateTime ActivationEnd { get; set; }
    public string Secret { get; set; }
}
