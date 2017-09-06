namespace Runner
{
    internal class AssertFail
    {
        public Stub Stub { get; private set; }
        public string Error { get; private set; }

        public AssertFail(Stub stub, string error)
        {
            Stub = stub;
            Error = error;
        }
    }
}