using Godot;

public partial class MainMenu : BoxContainer
{
    [Export] LineEdit ipInput;
    [Export] Button joinButton;
    [Export] Button hostButton;
    [Export] Button singleplayerButton;
    [Export] SingleCarController singleCarController;
    [Export] TwoCarController twoCarController;
    [Export] Car car2;

    public override void _Ready()
    {
        hostButton.ButtonDown += OnHostButton;
        joinButton.ButtonDown += OnJoinButton;
        singleplayerButton.ButtonDown += OnSinglePlayerButton;
    }

    private void OnHostButton()
    {
        twoCarController.StartHost();
        singleCarController.QueueFree();
        Hide();
    }

    private void OnJoinButton()
    {
        twoCarController.StartClient(ipInput.Text);
        singleCarController.QueueFree();
        Hide();
    }

    private void OnSinglePlayerButton()
    {
        singleCarController.Start();
        twoCarController.QueueFree();
        car2.QueueFree();
        Hide();
    }
}
