namespace RegisterService.Entity
{
    public record class Player
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required int Active { get; set; }
    }
}
