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
            })

            $("#myGroups-tab").click(() => {
                if (!this.myGroupsClicked) {
                    this.myGroupsCallback(1);
                    this.myGroupsClicked = true;
                }
            })
        },
        methods: {
            historyListCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl('/api/getQuizList', {
                    page: page,
                    size: global.quizCardAmount,
                    type: 'history',
                    username: model.userName
                })).then(function(response) {
                    self.historyList.entries = response.data.entries;
                    self.historyList.pages = response.data.pages;
                    self.historyList.showSummary = response.data.showSummary;
                });
            },
            myQuizzesCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl('/api/getQuizList', {
                    page: page,
                    size: global.quizCardAmount,
                    type: 'quizzesBy',
                    username: model.userName
                })).then(function(response) {
                    self.myQuizzes.entries = response.data.entries;
                    self.myQuizzes.pages = response.data.pages;
                    self.myQuizzes.canEdit = response.data.canEdit;
                });
            },
            myGroupsCallback: function(page) {
                let self = this;

                axios.get(util.apiUrl('/api/group/getList/myGroups', {
                    page: page,
                    size: global.membersCardAmount,
                    username: model.userName
                })).then(function(response) {
                    console.log(response.data);
                    self.myGroups.entries = response.data.entries;
                    self.myGroups.pages = response.data.pages;
                }).catch(function(e) {
                    if (e.response.status === 400)
                        util.openModal(e.response.data);
                });
            }
        }
    });
});