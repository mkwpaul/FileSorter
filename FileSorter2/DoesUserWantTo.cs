using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSorter
{
    public static class DoesUserWantTo
    {
        public static bool CreateFolder(string newFolderFull)
        {
            var model = new MessageBoxModel()
            {
                Caption = "Question",
                Text = $"Do you want to create a new Folder at: \n\n {newFolderFull}",
                Buttons = MessageBoxButtons.YesNo(),
            };

            model.SetDefaultButton(MessageBoxResult.Yes);

            var answer = MessageBox.Show(model) == MessageBoxResult.Yes;
            return answer;
        }

        public static void SetDefault(this MessageBoxModel model, object id)
        {
            var yesBtn = model.Buttons.FirstOrDefault(x => x.Id.Equals(id));
            if (yesBtn != null)
                yesBtn.IsDefault = true;
        }

        public static bool MoveFileToNewFolder()
        {
            var model = new MessageBoxModel()
            {
                Caption = "Question",
                Text = "Do you want to move the current file there?",
                Buttons = MessageBoxButtons.YesNo(),
            };
            model.SetDefaultButton(MessageBoxResult.Yes);

            var answer = MessageBox.Show(model) == MessageBoxResult.Yes;
            return answer;
        }

        public static bool DeleteFile()
        {
            var model = new MessageBoxModel
            {
                Text = "Are you sure you want to delete?",
                Buttons = MessageBoxButtons.YesNo(),
                Icon = MessageBoxImage.Exclamation,
                Caption = "File Deletion",
            };

            model.SetDefaultButton(MessageBoxResult.Yes);
            var answer = MessageBox.Show(model);
            return answer == MessageBoxResult.Yes;
        }

        public static FileCollisionReaction ReactToFileCollision(string newFullPath)
        {
            var model = new MessageBoxModel()
            {
                Caption = "Question",
                Text = $"A file already exists at {newFullPath}. What do you want to do?",
                Buttons = new List<IMessageBoxButtonModel>
                {
                     MessageBoxButtons.Custom("Overwrite File", FileCollisionReaction.Overwrite),
                     MessageBoxButtons.Custom("Delete current File", FileCollisionReaction.Delete),
                     MessageBoxButtons.Custom("Cancel", FileCollisionReaction.Cancel),
                },
                Icon = MessageBoxImage.Question
            };

            model.SetDefault(FileCollisionReaction.Overwrite);
            return (FileCollisionReaction)MessageBox.Show(model);
        }
    }

    public enum FileCollisionReaction
    {
        Cancel,
        Overwrite,
        Delete,
    }
}
