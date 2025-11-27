namespace BlueDotBrigade.Analyzers.Utilities
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PathHelperTests
    {
        [TestMethod]
        public void Normalize_ReturnsNull_WhenInputIsNull()
        {
            var result = PathHelper.Normalize(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Normalize_ReturnsEmptyString_WhenInputIsEmpty()
        {
            var result = PathHelper.Normalize(string.Empty);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Normalize_ReturnsWhitespace_WhenInputIsWhitespace()
        {
            var result = PathHelper.Normalize("   ");

            Assert.AreEqual("   ", result);
        }

        [TestMethod]
        public void Normalize_ReplacesAltSeparator_WithStandardSeparator()
        {
            var input = "folder" + System.IO.Path.AltDirectorySeparatorChar + "subfolder";
            var expected = "folder" + System.IO.Path.DirectorySeparatorChar + "subfolder";

            var result = PathHelper.Normalize(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Normalize_RemovesTrailingSeparator()
        {
            var input = "folder" + System.IO.Path.DirectorySeparatorChar + "subfolder" + System.IO.Path.DirectorySeparatorChar;
            var expected = "folder" + System.IO.Path.DirectorySeparatorChar + "subfolder";

            var result = PathHelper.Normalize(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Normalize_TrimsWhitespace()
        {
            var input = "  folder  ";
            var expected = "folder";

            var result = PathHelper.Normalize(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TrimEndingDirectorySeparator_ReturnsNull_WhenInputIsNull()
        {
            var result = PathHelper.TrimEndingDirectorySeparator(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void TrimEndingDirectorySeparator_ReturnsEmptyString_WhenInputIsEmpty()
        {
            var result = PathHelper.TrimEndingDirectorySeparator(string.Empty);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void TrimEndingDirectorySeparator_ReturnsPath_WhenNoTrailingSeparator()
        {
            var input = "folder" + System.IO.Path.DirectorySeparatorChar + "subfolder";

            var result = PathHelper.TrimEndingDirectorySeparator(input);

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TrimEndingDirectorySeparator_RemovesMultipleTrailingSeparators()
        {
            var sep = System.IO.Path.DirectorySeparatorChar;
            var input = "folder" + sep + "subfolder" + sep + sep + sep;
            var expected = "folder" + sep + "subfolder";

            var result = PathHelper.TrimEndingDirectorySeparator(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IsDirectorySeparator_ReturnsTrue_ForStandardSeparator()
        {
            var result = PathHelper.IsDirectorySeparator(System.IO.Path.DirectorySeparatorChar);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsDirectorySeparator_ReturnsTrue_ForAltSeparator()
        {
            var result = PathHelper.IsDirectorySeparator(System.IO.Path.AltDirectorySeparatorChar);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsDirectorySeparator_ReturnsFalse_ForNonSeparatorChar()
        {
            var result = PathHelper.IsDirectorySeparator('a');

            Assert.IsFalse(result);
        }
    }
}
