<%@ Page Title="Manage Questions" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageQuestions.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageQuestions" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Manage Questions</h1>

    <div class="row mb-4">
        <div class="col-md-5">
            <label class="form-label" for="ddlQuiz">Quiz</label>
            <asp:DropDownList ID="ddlQuiz" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlQuiz_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <asp:GridView ID="grdQuestions" runat="server" AutoGenerateColumns="false" CssClass="table table-striped mb-4"
            DataKeyNames="QuestionID" OnRowCommand="grdQuestions_RowCommand" EmptyDataText="No questions yet.">
            <Columns>
                <asp:BoundField DataField="QuestionText" HeaderText="Question" />
                <asp:BoundField DataField="Marks" HeaderText="Marks" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton runat="server" Text="Edit" CommandName="EditQuestion" CommandArgument='<%# Eval("QuestionID") %>' CssClass="btn btn-sm btn-outline-primary" />
                        <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteQuestion" CommandArgument='<%# Eval("QuestionID") %>' CssClass="btn btn-sm btn-outline-danger"
                            OnClientClick="return confirm('Delete this question and its answers?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <div class="card">
            <div class="card-body">
                <h5 class="card-title" id="lblEditorHeading" runat="server">New question</h5>
                <asp:HiddenField ID="hidQuestionId" runat="server" />
                <div class="row g-3">
                    <div class="col-12">
                        <label class="form-label" for="txtQuestionText">Question text</label>
                        <asp:TextBox ID="txtQuestionText" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="col-md-3">
                        <label class="form-label" for="txtMarks">Marks</label>
                        <asp:TextBox ID="txtMarks" runat="server" CssClass="form-control" Text="1" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMarks"
                            ValidationExpression="^[1-9][0-9]*$" ErrorMessage="Whole number." CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>

                <h6 class="mt-4">Answer options (tick the correct one(s))</h6>
                <div class="row g-2">
                    <asp:Repeater ID="rptAnswerRows" runat="server">
                        <ItemTemplate>
                            <div class="col-md-6">
                                <div class="input-group">
                                    <span class="input-group-text">
                                        <asp:CheckBox ID="chkCorrect" runat="server" />
                                    </span>
                                    <asp:TextBox ID="txtAnswer" runat="server" CssClass="form-control" Placeholder='Answer <%# Container.ItemIndex + 1 %>' />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" />
                <div class="mt-3">
                    <asp:Button ID="btnSave" runat="server" Text="Save question" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>