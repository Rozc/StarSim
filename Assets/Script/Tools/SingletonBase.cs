namespace Script.Tools
{
    public class SingletonBase<T> where T : class, new()
    {
        private static T instance;
        private static readonly object locker = new object();
        protected SingletonBase() { }
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }
                return instance;
            }
        }
    }

}
