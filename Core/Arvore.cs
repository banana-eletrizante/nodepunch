using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

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
                string[] directories = Directory.GetDirectories(path)
                    .OrderBy(directory => directory, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                foreach (string dir in directories)
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    if (!di.Name.Equals("node_modules", StringComparison.OrdinalIgnoreCase) &&
                        !di.Name.Equals(".git", StringComparison.OrdinalIgnoreCase) &&
                        !di.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        TreeNode child = new TreeNode(di.Name) { Tag = dir };
                        node.Nodes.Add(child);
                        LoadSubDirectoriesAndFiles(dir, child);
                    }
                }

                string[] files = Directory.GetFiles(path)
                    .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
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
            string path = selectedNode.Tag?.ToString();
            string raiz = arvore.Nodes.Count > 0 ? arvore.Nodes[0].Tag?.ToString() : null;

            if (!EhCaminhoSeguroParaExcluir(raiz, path))
            {
                MessageBox.Show(
                    "A pasta raiz do projeto não pode ser excluída pelo NodePunch.",
                    "Operação bloqueada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            path = Path.GetFullPath(path);

            try
            {
                if (File.Exists(path))
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                else if (Directory.Exists(path))
                    FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
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

        internal static bool EhCaminhoSeguroParaExcluir(string raiz, string alvo)
        {
            if (string.IsNullOrWhiteSpace(raiz) || string.IsNullOrWhiteSpace(alvo))
                return false;

            try
            {
                string raizCompleta = Path.GetFullPath(raiz)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string alvoCompleto = Path.GetFullPath(alvo);
                string prefixoRaiz = raizCompleta + Path.DirectorySeparatorChar;

                return !alvoCompleto.Equals(raizCompleta, StringComparison.OrdinalIgnoreCase) &&
                       alvoCompleto.StartsWith(prefixoRaiz, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                return false;
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
