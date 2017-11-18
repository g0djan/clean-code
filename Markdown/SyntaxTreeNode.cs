using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Markdown
{
    public class SyntaxTreeNode
    {
        private List<SyntaxTreeNode> childs;
        public TagType Tag { get; }
        private string leafContent;

        public SyntaxTreeNode(TagType tag)
        {
            Tag = tag;
            childs = new List<SyntaxTreeNode>();
        }

        public List<SyntaxTreeNode> BuildSyntaxTree(Lexeme[] lexemes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SyntaxTreeNode> GetAllChilds()
        {
            throw new NotImplementedException();
        }

        private void AccumulateLeafContent(State state)
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class SyntaxTreeNode_Should
    {
        [Test]
        public void DoSomething_WhenSomething()
        {

        }
    }
}