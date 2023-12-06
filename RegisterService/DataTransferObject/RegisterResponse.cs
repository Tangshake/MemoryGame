namespace RegisterService.DataTransferObject
{
    public class RegisterResponse
    {
        public required int Id { get; set; }
        public bool RegisterSuccess { get; set; } = false;
        public required String Message { get; set; }
    }
}
