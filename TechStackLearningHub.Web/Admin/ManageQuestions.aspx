<%@ Page Title="Manage Questions" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageQuestions.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageQuestions" %>
<%@ Import Namespace="TechStackLearningHub.Web.Helpers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row g-3 mb-4">
        <div class="col-md-5">
            <asp:Label runat="server" AssociatedControlID="ddlQuiz" CssClass="form-label" Text="Quiz" />
            <asp:DropDownList ID="ddlQuiz" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlQuiz_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <div class="row g-3">
            <div class="col-lg-8">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <span>Questions</span>
                    </div>
                    <div class="card-body p-0">
                        <asp:GridView ID="grdQuestions" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0"
                            DataKeyNames="QuestionID" OnRowCommand="grdQuestions_RowCommand" EmptyDataText="No questions yet.">
                            <Columns>
                                <asp:BoundField DataField="QuestionText" HeaderText="Question" />
                                <asp:BoundField DataField="Marks" HeaderText="Marks" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" Text="Edit" CommandName="EditQuestion" CommandArgument='<%# Eval("QuestionID") %>' CssClass="btn btn-sm btn-outline-primary" />
                                        <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteQuestion" CommandArgument='<%# Eval("QuestionID") %>' CssClass="btn btn-sm btn-outline-danger"
                                            OnClientClick='<%# AdminUi.Confirm("Delete the question {0} and its answers?", Eval("QuestionText")) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
            <div class="col-lg-4">
                <div class="card">
                    <div class="card-header">
                        <h2 class="h5 mb-0" id="lblEditorHeading" runat="server">New question</h2>
                    </div>
                    <div class="card-body">
                        <asp:HiddenField ID="hidQuestionId" runat="server" />
                        <div class="row g-3">
                            <div class="col-12">
                                <asp:Label runat="server" AssociatedControlID="txtQuestionText" CssClass="form-label" Text="Question text" />
                                <asp:TextBox ID="txtQuestionText" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                            </div>
                            <div class="col-12">
                                <asp:Label runat="server" AssociatedControlID="txtMarks" CssClass="form-label" Text="Marks" />
                                <asp:TextBox ID="txtMarks" runat="server" CssClass="form-control" Text="1" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMarks"
                                    ValidationExpression="^[1-9][0-9]*$" ErrorMessage="Whole number." CssClass="text-danger" Display="Dynamic" />
                            </div>
                        </div>

                        <h3 class="h6 mt-4">Answer options (tick the correct one(s))</h3>
                        <div class="row g-2">
                            <asp:Repeater ID="rptAnswerRows" runat="server">
                                <ItemTemplate>
                                    <div class="col-12">
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

                        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" role="alert" />
                        <asp:Label ID="lblSuccess" runat="server" CssClass="text-success d-block mt-2" Visible="false" role="status" />
                        <div class="mt-3">
                            <asp:Button ID="btnSave" runat="server" Text="Save question" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
