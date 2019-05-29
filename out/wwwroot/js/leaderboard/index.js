$(document).ready(function () {
    let app = new Vue({
        el: '#tabContent',
        data: {
            quiz: quizBoard ? model : undefined,
            size: 10,
            today: {
                currentPage: 1,
                entries: [],
                pages: 0
            },
            week: {
                currentPage: 1,
                entries: [],
                pages: 0
            },
            month: {
                currentPage: 1,
                entries: [],
                pages: 0  
            },
            all: {
                currentPage: 1,
                entries: [],
                pages: 0
            },
            weekTabClicked: false,
            monthTabClicked: false,
            allTimeTabClicked: false,
        },
        mounted: function() {
            this.getToday(1);

            $("#week-tab").click(() => {
                if (!this.weekTabClicked) {
                    this.weekTabClicked = true;
                    this.getWeek(1);
                }
            });

            $("#month-tab").click(() => {
                if (!this.monthTabClicked) {
                    this.monthTabClicked = true;
                    this.getMonth(1);
                }
            });

            $("#allTime-tab").click(() => {
                if (!this.allTimeTabClicked) {
                    this.allTimeTabClicked = true;
                    this.getAllTime(1);
                }
            });
        },
        methods: {
            getAllTime: function(p) {
                this.getLeaderboard(p, 'all')
            },
            getToday: function(p) {
                this.getLeaderboard(p, 'today')
            },
            getWeek: function(p) {
                this.getLeaderboard(p, 'week')
            },
            getMonth: function(p) {
                this.getLeaderboard(p, 'month')
            },
            getLeaderboard: function(page, type) {
                let self = this;
                axios.get(util.apiUrl(`/api/leaderboard/getList/${type}`, {
                    page: page,
                    size: self.size,
                    id: quizBoard ? model.id : undefined
                })).then(function(response) {
                    if (global.debug) util.logResponse(response);
                    
                    self[type].entries = response.data.entries;
                    self[type].pages = response.data.pages;
                    self[type].currentPage = page;
                }).catch(e => util.logResponse(e.response));
            }
        }
    });
});