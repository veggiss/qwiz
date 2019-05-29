$(document).ready(() => {
    $(".progress-bar").css("width", ((model.xp) / (model.xpNeeded)) * 100 + "%");
    
    let app = new Vue({
        el: '#tabContent',
        data: {
            myQuizzes: {
                entries: [],
                pages: 0,
                canEdit: false
            },
            historyList: {
                entries: [],
                pages: 0,
                showSummary: false,
            },
            myGroups: {
                entries: [],
                pages: 0
            },
            myQuizzesClicked: false,
            myGroupsClicked: false
        },
        mounted: function() {
            this.historyListCallback(1);
            $("#myQuizzes-tab").click(() => {
                if (!this.myQuizzesClicked) {
                    this.myQuizzesCallback(1);
                    this.myQuizzesClicked = true;
                }
            });

            $("#myGroups-tab").click(() => {
                if (!this.myGroupsClicked) {
                    this.myGroupsCallback(1);
                    this.myGroupsClicked = true;
                }
            });
        },
        methods: {
            historyListCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl('/api/quiz/getList/history', {
                    page: page,
                    size: global.quizCardAmount,
                    username: model.userName
                })).then(function(response) {
                    if (global.debug) util.logResponse(response);
                    self.historyList.entries = response.data.entries;
                    self.historyList.pages = response.data.pages;
                    self.historyList.showSummary = response.data.showSummary;
                }).catch(e => util.logResponse(e.response));
            },
            myQuizzesCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl(`/api/quiz/getList/quizzesBy`, {
                    page: page,
                    size: global.quizCardAmount,
                    username: model.userName
                })).then(function(response) {
                    if (global.debug) util.logResponse(response);
                    self.myQuizzes.entries = response.data.entries;
                    self.myQuizzes.pages = response.data.pages;
                    self.myQuizzes.canEdit = response.data.canEdit;
                }).catch(e => util.logResponse(e.response));
            },
            myGroupsCallback: function(page) {
                let self = this;

                axios.get(util.apiUrl(`/api/group/getList/user`, {
                    page: page,
                    size: global.membersCardAmount,
                    username: model.userName
                })).then(function(response) {
                    if (global.debug) util.logResponse(response);
                    self.myGroups.entries = response.data.entries;
                    self.myGroups.pages = response.data.pages;
                }).catch(e => util.logResponse(e.response));
            }
        }
    });
});