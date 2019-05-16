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
            }
        },
        mounted: function() {
            this.historyListCallback(1);
            this.myQuizzesCallback(1);
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
            badgesCallback: function(page) {

            }
        }
    });
});