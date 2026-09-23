<%@ Page Title="Quiz" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuizPage.aspx.cs" Inherits="TechStackLearningHub.Web.Student.QuizPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlQuiz" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1 class="mb-0"><asp:Literal ID="litQuizTitle" runat="server" /></h1>
            <span id="quizTimer" class="fs-4 fw-bold text-danger"></span>
        </div>
        <p class="text-muted">Points are awarded only for exactly matching all correct answers. The quiz is submitted automatically when the timer reaches zero.</p>

        <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
            <ItemTemplate>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="d-flex justify-content-between">
                            <h5 class="card-title"><%# Eval("QuestionText") %></h5>
                            <span class="badge bg-secondary"><%# Eval("Marks") %> mark(s)</span>
                        </div>
                        <asp:Literal ID="litQuestionId" runat="server" Visible="false" Text='<%# Eval("QuestionID") %>' />
                        <asp:PlaceHolder ID="phAnswers" runat="server" />
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Button ID="btnSubmitQuiz" runat="server" Text="Submit Quiz" CssClass="btn btn-primary btn-lg"
            OnClick="btnSubmitQuiz_Click" OnClientClick="return confirm('Submit quiz? You cannot change answers afterwards.');" />
    </asp:Panel>

    <asp:Panel ID="pnlResult" runat="server" Visible="false">
        <div class="mt-4">
            <div class="card">
                <div class="card-body text-center">
                    <h2>
                        <asp:Label ID="litOutcome" runat="server" />
                    </h2>
                    <p class="fs-4">
                        Your score: <asp:Literal ID="litScore" runat="server" />
                    </p>
                    <p class="fs-5">
                        Required to pass: <asp:Literal ID="litPassMark" runat="server" />
                    </p>
                    <a class="btn btn-outline-primary" id="lnkBackToCourse" runat="server">Back to course</a>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlRetake" runat="server" Visible="false">
        <p class="text-danger">You have already passed this quiz. You can review the course again, but you may not retake it.</p>
    </asp:Panel>

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="alert alert-warning">This quiz is not available.</div>
    </asp:Panel>

    <script type="text/javascript">
        (function () {
            var seconds = 600;
            var timerEl = document.getElementById('quizTimer');
            var form = document.getElementById('aspnetForm') || null;
            var btn = document.getElementById('<%= btnSubmitQuiz.ClientID %>');
            function fmt(s) {
                var m = Math.floor(s / 60);
                var ss = s % 60;
                return m + ':' + (ss < 10 ? '0' : '') + ss;
            }
            if (timerEl) {
                timerEl.textContent = fmt(seconds);
                var iv = setInterval(function () {
                    seconds--;
                    if (seconds <= 0) {
                        clearInterval(iv);
                        timerEl.textContent = fmt(0);
                        if (btn && form) {
                            btn.disabled = true;
                            form.submit();
                        }
                    } else {
                        timerEl.textContent = fmt(seconds);
                    }
                }, 1000);
            }
        })();
    </script>
</asp:Content>