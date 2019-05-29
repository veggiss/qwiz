$(document).ready(function() {
    new Vue({
        el: '#quizForm',
        data: {
            isEdit: model !== null,
            question: {
                "questionType": "multiple_choice",
                "questionText": "",
                "correctAnswer": "",
                "correctAlternative": "A",
                "difficulty": "easy",
                "imagePath": "",
                "alternatives": ["", "", "", ""]
            },
            questions: []
        },
        mounted: function() {
            $("#btnAddQuestion").click(() => this.addQuestion());
            $("#btnCreateQuiz").click(() => this.submitQuiz());
            $("#quizImg").change(e => this.changeQuizImg(e));
            $("#btnDeleteQuiz").click(() => this.deleteQuiz());
            $(".alert .close").click(() => $(".alert").hide());
            
            if (this.isEdit) {
                $('#quizCategory option').filter(function() {
                    return $(this).text() === model.category;
                }).prop('selected', true);
                $("#quizTitle").val(model.topic);
                $("#quizDifficulty").val(model.difficulty);
                $("#quizImgPrev").attr('src', model.imagePath);
                $("#quizDescription").val(model.description);
                
                model.questions.forEach((e, i) => {
                    delete e.id;
                    if (e.questionType === "multiple_choice") e.alternatives = JSON.parse(e.alternatives);
                    this.$set(this.questions, i, e);
                });
            }
        },
        methods: {
            onCollapse: function(e) {
                $(e.target).next().slideToggle("slow", "swing");
            },
            addQuestion: function() {
                this.questions.push(this.question);
            },
            submitQuiz: function() {
                if (this.questions.length > 0 && $("#quizForm")[0].checkValidity()) {
                    let api = '/api/quiz/create';
                    let quiz = {
                        topic: $("#quizTitle").val(),
                        difficulty: $("#quizDifficulty").find(":selected").val(),
                        category: $("#quizCategory").find(":selected").val(),
                        imagePath: $("#quizImgPrev").attr('src'),
                        description: $("#quizDescription").val(),
                        questions: this.questionsToJson()
                    };

                    if (this.isEdit) {
                        quiz.id = model.id;
                        api = '/api/quiz/update';
                    }
                    
                    axios.post(api, JSON.stringify(quiz), global.header).then(function(response) {
                        if (global.debug) util.logResponse(response);
                        if (response.status === 200) window.location.href = '/user';
                    }).catch(e => util.logResponse(e.response));
                } else {
                    $(".alert").show().delay(2000).fadeOut();
                }
            },
            onChange: function(type, i, e) {
                let question = $.extend(true, {}, this.questions[i]);
                
                switch (type) {
                    case 'text':
                        question.questionText = e.target.value;
                    break;
                    case 'difficulty':
                        question.difficulty = e.target.value;
                    break;
                    case 'type':
                        let questionType = $(e.target).find(":selected").val();
                        question.questionType = questionType;
                        if (questionType === 'multiple_choice') {
                            question.correctAlternative = 'A';
                        } else if(questionType === 'true_false') {
                            question.correctAlternative = 'T';
                        }
                    break;
                    case 'answer':
                        question.correctAlternative = e.target.value;
                    break;
                    case 'image':
                        util.uploadImage(e.target.files[0])
                            .then(path => question.imagePath = path)
                            .catch(e => util.logResponse(e.response));
                    break;
                    case 'alternative':
                        let altIndex = parseInt($(e.target).attr("index"));
                        question.alternatives[altIndex] = e.target.value;
                    break;
                }

                this.$set(this.questions, i, question);
            },
            changeQuizImg: function(e) {
                util.uploadImage(e.target.files[0])
                    .then(path => $("#quizImgPrev").attr("src", path))
                    .catch(e => util.logResponse(e.response));
            },
            questionsToJson: function() {
                let questions = [];
                this.questions.forEach(e => {
                    let q = $.extend(true, {}, e);

                    switch (q.correctAlternative) {
                        case 'A':
                            q.correctAnswer = q.alternatives[0];
                            break;
                        case 'B':
                            q.correctAnswer = q.alternatives[1];
                            break;
                        case 'C':
                            q.correctAnswer = q.alternatives[2];
                            break;
                        case 'D':
                            q.correctAnswer = q.alternatives[3];
                            break;
                        case 'T':
                            q.correctAnswer = 'True';
                            break;
                        case 'F':
                            q.correctAnswer = 'False';
                            break;
                    }
                    
                    q.alternatives = q.questionType === "multiple_choice" ? JSON.stringify(q.alternatives) : null;
                    questions.push(q);
                });
                
                return questions;
            },
            deleteQuiz: function() {
                if (this.isEdit && global.isAuthenticated) {
                    axios.delete('/api/quiz/delete/' + model.id, global.header).then(function(response) {
                        if (global.debug) util.logResponse(response);
                        if (response.status === 200) window.location.href = "/quiz";
                    }).catch(e => util.logResponse(e.response));
                }
            }
        }
    });
});