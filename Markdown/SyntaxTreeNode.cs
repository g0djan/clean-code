using System;
using System.Collections.Generic;

namespace Markdown
{
    public class SyntaxTreeNode
    {
        private List<SyntaxTreeNode> childs;
        public Tag Tag { get; }

        public SyntaxTreeNode(Tag tag)
        {
            Tag = tag;
            childs = BuildSyntaxTree();
        }

        public List<SyntaxTreeNode> BuildSyntaxTree(Lexeme[] lexemes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SyntaxTreeNode> GetAllChilds()
        {
            throw new NotImplementedException();
        }
    }
}