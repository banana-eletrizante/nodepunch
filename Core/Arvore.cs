using System;
using System.IO;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class Arvore
    {
        public static void LoadDirectoryTree(string path, TreeView treeView)
        {
            treeView.Nodes.Clear();
            TreeNode node = new TreeNode(new DirectoryInfo(path).Name)
            {
                Tag = path
            };
            treeView.Nodes.Add(node);
            LoadSubDirectoriesAndFiles(path, node);
        }

        private static void LoadSubDirectoriesAndFiles(string path, TreeNode node)
        {
            try
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string dir in directories)
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    if (!di.Name.Equals("node_modules", StringComparison.OrdinalIgnoreCase) &&
                        !di.Name.Equals(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        TreeNode child = new TreeNode(di.Name) { Tag = dir };
                        node.Nodes.Add(child);
                        LoadSubDirectoriesAndFiles(dir, child);
                    }
                }

                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    TreeNode child = new TreeNode(fi.Name) { Tag = file };
                    node.Nodes.Add(child);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        public static void ExcluirItemSelecionado(TreeView arvore)
        {
            if (arvore.SelectedNode == null)
            {
                MessageBox.Show("Selecione um item para excluir.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            TreeNode selectedNode = arvore.SelectedNode;
            string path = selectedNode.Tag.ToString();
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
                else
                {
                    MessageBox.Show("O caminho selecionado não existe mais.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                selectedNode.Remove();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public static void ExpandirNos(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                switch (node.Text.ToLower())
                {
                    case "src":
                    case "base":
                    case "controllers":
                    case "models":
                    case "routes":
                        node.Expand();
                        break;
                    default:
                        node.Collapse();
                        break;
                }
                ExpandirNos(node.Nodes);
            }
        }
    }
}
