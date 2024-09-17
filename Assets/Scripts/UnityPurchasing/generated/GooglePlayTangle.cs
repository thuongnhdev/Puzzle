// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("5VAZSAdOuVki4jPC8gaQqJB7pKdVM+OX3A/NSZ5N8+hvFhpWgG04fn9a6J5IBeMZAmJrwdtp2zRJl34+C3SCl4ldNk+jm7ZAxI9awJXLkaQ/wuG59DTiRmzlly2pBNaCzInkioWQVsXgswSiocTDK+6rbKNGFx5Q6mlnaFjqaWJq6mlpaMiNdT10ld5Y6mlKWGVuYULuIO6fZWlpaW1oa4ByXSDX2mb+x1aPqtJCJgPwTzQHjGCKGB8OBTaWuI6UFWfmzDwfb8Lri1ZSJBIdJqQqfI7hQ9Kfqm1e1Yt+yG1lGsqfHPKBT/IlDXC+qGoiCTRy3zQ4TeJVBhHnbR+NVSfgqsijxvOAhJgcmKyvANheYi3u/fd1KWiz6ns/4lUrj2praWhp");
        private static int[] order = new int[] { 2,8,11,4,4,9,8,11,10,10,10,13,12,13,14 };
        private static int key = 104;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
