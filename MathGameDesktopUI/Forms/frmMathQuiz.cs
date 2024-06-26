﻿namespace Math_Quiz
{
    public partial class frmMathQuiz : Form
    {
        public enum enLevel
        {
            Easy = 1,
            Medium = 2,
            Hard = 3,
            Mixed = 4
        };
        public enum enOperation
        {
            Addition = 1,
            Subtraction = 2,
            Multiplication = 3,
            Division = 4,
            opMixed = 5
        };

        struct stQuizInfo
        {
            public byte NumberOfQuestions;
            public enOperation Operation;
            public enLevel Level;
            public ushort TimerInSec;
        }
        stQuizInfo QuizInfo;

        private List<ucQuestion> _lstQuestions = new List<ucQuestion>();

        private static Random _Random = new Random();

        private ushort _TimerInSec = 0;

        private frmMainMenu _MainMenuForm;
        private bool _IsGameFinished = false;
        private bool _isClosing = false;

        public frmMathQuiz(byte NumberOfQuestions, enOperation Operation, enLevel Level, ushort TimerInMin, frmMainMenu MainMenuForm)
        {
            InitializeComponent();

            QuizInfo = new stQuizInfo();

            QuizInfo.NumberOfQuestions = NumberOfQuestions;
            QuizInfo.Operation = Operation;
            QuizInfo.Level = Level;
            QuizInfo.TimerInSec = _TimerInSec = (ushort)(TimerInMin * 60);

            _MainMenuForm = MainMenuForm;

            timer1.Start();
        }

        private void _DisableTabStopOfAllUserControlsInList()
        {
            _lstQuestions.ForEach(q => q.DisableTabStop = false);
        }

        private enOperation _GetCurrentOperation()
        {
            enOperation CurrentOperation = QuizInfo.Operation;

            if (QuizInfo.Operation == enOperation.opMixed)
            {
                CurrentOperation = (enOperation)_Random.Next(1, 5);
            }

            return CurrentOperation;
        }

        private double[] _GetRandomNumberAccordingToQuestionLevel(out enLevel CurrentLevel)
        {
            double[] TwoRandomNumbers = new double[2];

            CurrentLevel = QuizInfo.Level;

            if (CurrentLevel == enLevel.Mixed)
                CurrentLevel = (enLevel)_Random.Next(1, 4);

            switch (CurrentLevel)
            {
                case enLevel.Easy:
                    TwoRandomNumbers[0] = _Random.Next(0, 10);
                    TwoRandomNumbers[1] = _Random.Next(0, 10);
                    break;

                case enLevel.Medium:
                    TwoRandomNumbers[0] = _Random.Next(10, 100);
                    TwoRandomNumbers[1] = _Random.Next(10, 100);
                    break;

                case enLevel.Hard:
                    TwoRandomNumbers[0] = _Random.Next(100, 999);
                    TwoRandomNumbers[1] = _Random.Next(100, 999);
                    break;

                case enLevel.Mixed:
                    TwoRandomNumbers[0] = _Random.Next(0, 999);
                    TwoRandomNumbers[1] = _Random.Next(0, 999);
                    break;

            }

            return TwoRandomNumbers;
        }

        private ucQuestion _CreateQuestion(byte questionNumber)
        {
            ucQuestion Question = new ucQuestion();
            double[] TwoRandomNumbers = _GetRandomNumberAccordingToQuestionLevel(out enLevel CurrentLevel);

            Question.MakeQuestion(TwoRandomNumbers[0], TwoRandomNumbers[1], _GetCurrentOperation(), CurrentLevel);

            Question.QuestionNumber = questionNumber;
            return Question;
        }

        private void _CreateQuestions()
        {
            flpQuestions.Controls.Clear();
            _lstQuestions.Clear();

            ucQuestion Question;

            for (byte i = 1; i <= QuizInfo.NumberOfQuestions; i++)
            {
                Question = _CreateQuestion(i);

                _lstQuestions.Add(Question);

                flpQuestions.Controls.Add(Question);
            }
        }

        private byte _CalculateFinalMark()
        {
            if (_lstQuestions.Count == 0)
                return 0;

            return (byte)_lstQuestions.Count(q => q.IsAnswerCorrect);
        }

        private void _ShowEffectOfOptionsAndDisableUnselectedOptions()
        {
            if (_lstQuestions.Count == 0)
                return;

            _lstQuestions.ForEach(q =>
            {
                q.ShowEffectOfOptions();
                q.DisableUnselectedOptions();
            });
        }

        private void _UpdateUIWithResult(byte FinalMark)
        {
            lblNumberOfQuestions.Visible = lblResult.Visible = lblSlash.Visible = lblYourRestulText.Visible = true;

            lblNumberOfQuestions.Text = QuizInfo.NumberOfQuestions.ToString();
            lblResult.Text = FinalMark.ToString();

            if (FinalMark >= QuizInfo.NumberOfQuestions / 2)
            {
                lblResult.ForeColor = Color.LimeGreen;
                MessageBox.Show($"Great! you got {FinalMark}/{QuizInfo.NumberOfQuestions}," +
                    $" Well done!", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblResult.ForeColor = Color.OrangeRed;
                MessageBox.Show($"You got {FinalMark}/{QuizInfo.NumberOfQuestions}",
                    "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            _ShowEffectOfOptionsAndDisableUnselectedOptions();
        }

        private void _PrintResult()
        {
            byte FinalMark = _CalculateFinalMark();
            _UpdateUIWithResult(FinalMark);

            _IsGameFinished = true;
        }

        private bool _ShowExitConfirmationDialog()
        {
            return MessageBox.Show("Are you sure you want to exit the quiz? Your answers will be lost.",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.No;
        }

        private void _ExitGame()
        {
            timer1.Stop();
            _MainMenuForm.Show();
            this.Hide();
        }

        private void frmMathQuiz_Activated(object sender, EventArgs e)
        {
            if (_lstQuestions.Count == 0)
            {
                _CreateQuestions();
                //AddLabelsToFlowLayoutPanel();
                _DisableTabStopOfAllUserControlsInList();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Decrement the remaining time by 1 second
            _TimerInSec--;

            // Update the display with the remaining time
            TimeSpan timeSpan = TimeSpan.FromSeconds(_TimerInSec);
            lblTimer.Text = timeSpan.ToString(@"mm\:ss");

            if (_TimerInSec <= 10)
                lblTimer.ForeColor = Color.Red;

            // Check if the timer has reached 0
            if (_TimerInSec <= 0)
            {
                timer1.Stop();
                _PrintResult();
                // Perform any actions you need when the timer expires
            }
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            timer1.Stop();

            if (MessageBox.Show("Are you sure you want to finish the quiz?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                timer1.Start();
                return;
            }

            _PrintResult();
        }

        private void btnReturnToMainMenu_Click(object sender, EventArgs e)
        {
            lblNumberOfQuestions.Visible = lblResult.Visible = lblSlash.Visible = lblYourRestulText.Visible = false;

            if (_IsGameFinished)
            {
                _ExitGame();
            }
            else
            {
                if (!_ShowExitConfirmationDialog())
                {
                    _isClosing = true;
                    _ExitGame();
                }
            }
        }

        private void frmMathQuiz_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isClosing)
            {
                return;
            }

            if (_IsGameFinished)
            {
                // close without showing the message
                e.Cancel = false;
                _ExitGame();
                return;
            }

            if (_ShowExitConfirmationDialog())
            {
                // Cancel the form closing event
                e.Cancel = true;
                _isClosing = false;
            }
            else
            {
                _ExitGame();
            }
        }
    }
}
