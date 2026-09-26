<%@ Page Title="Quiz" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="QuizPage.aspx.cs" Inherits="TechStackLearningHub.Web.Member.QuizPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlQuiz" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1 class="mb-0"><asp:Literal ID="litQuizTitle" runat="server" /></h1>
            <div class="text-end">
                <div id="quizTimer" class="fs-4 fw-bold text-danger" role="timer" aria-live="off"></div>
                <div class="small text-muted">Time remaining</div>
            </div>
        </div>
        <p class="text-muted" id="quizInstructions">Points are awarded only for exactly matching all correct answers. The quiz is submitted automatically when the timer reaches zero.</p>

        <div class="alert alert-warning d-none" id="quizTimeWarning" role="alert">
            Less than a minute left. Finish your answers now - the quiz submits itself when time runs out.
        </div>

        <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
            <ItemTemplate>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="d-flex justify-content-between">
                            <h2 class="h5 card-title"><%# Eval("QuestionText") %></h2>
                            <span class="badge bg-secondary"><%# Eval("Marks") %> mark(s)</span>
                        </div>
                        <asp:Literal ID="litQuestionId" runat="server" Visible="false" Text='<%# Eval("QuestionID") %>' />
                        <asp:PlaceHolder ID="phAnswers" runat="server" />
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Button ID="btnSubmitQuiz" runat="server" Text="Submit quiz" CssClass="btn btn-primary btn-lg"
            OnClick="btnSubmitQuiz_Click"
            OnClientClick="return confirm('Submit quiz? You cannot change your answers afterwards.');" />

        <script type="text/javascript">
            (function () {
                // The server owns the deadline. The page is rendered with the
                // absolute UTC instant the attempt expires, so reloading the page
                // cannot hand back a fresh 10 minutes.
                var deadlineUtcMs = <%= QuizDeadlineUtcMs %>;
                var durationSeconds = <%= QuizDurationSeconds %>;
                var warnAtSeconds = 60;

                var timerEl = document.getElementById('quizTimer');
                var warnEl = document.getElementById('quizTimeWarning');
                var btn = document.getElementById('<%= btnSubmitQuiz.ClientID %>');
                var intervalId = null;
                var submitted = false;

                function fmt(s) {
                    var m = Math.floor(s / 60);
                    var ss = s % 60;
                    return m + ':' + (ss < 10 ? '0' : '') + ss;
                }

                function remainingSeconds() {
                    var left = Math.round((deadlineUtcMs - new Date().getTime()) / 1000);
                    return left < 0 ? 0 : left;
                }

                function submitNow() {
                    if (submitted) return;
                    submitted = true;
                    if (intervalId) clearInterval(intervalId);
                    if (btn) btn.disabled = true;

                    // __doPostBack, not form.submit(). The attempt is only ever
                    // graded in btnSubmitQuiz_Click, and Web Forms raises that
                    // Click event purely from __EVENTTARGET naming this button.
                    // Native form.submit() posts no event target, so the old timer
                    // silently threw the whole attempt away at zero.
                    __doPostBack('<%= btnSubmitQuiz.UniqueID %>', '');
                }

                function tick() {
                    var left = remainingSeconds();
                    if (timerEl) {
                        timerEl.textContent = fmt(left);
                        timerEl.classList.toggle('text-danger', left <= warnAtSeconds);
                        timerEl.classList.toggle('text-success', left > warnAtSeconds);
                    }
                    if (warnEl) {
                        warnEl.classList.toggle('d-none', left > warnAtSeconds);
                    }
                    if (left <= 0) {
                        submitNow();
                    }
                }

                if (!timerEl) return;

                // Recomputing from the absolute deadline, instead of decrementing
                // a local counter, keeps the countdown honest when a browser
                // throttles setInterval in a background tab.
                tick();
                intervalId = setInterval(tick, 1000);

                document.addEventListener('visibilitychange', function () {
                    if (!document.hidden) tick();
                });

                window.addEventListener('beforeunload', function () {
                    if (intervalId) clearInterval(intervalId);
                });

                // Surfaced for the end-to-end harness: proves the page was handed
                // the server's deadline rather than a hardcoded 600.
                window.__quizDeadlineUtcMs = deadlineUtcMs;
                window.__quizDurationSeconds = durationSeconds;
            })();
        </script>
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
                    <div class="alert alert-warning d-none" id="quizExpiredNotice" role="alert">
                        Time ran out, so this attempt was submitted and graded automatically.
                    </div>
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
</asp:Content>