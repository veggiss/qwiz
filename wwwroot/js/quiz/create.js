$(document).ready(function() {
    new Vue({
        el: '#quizField',
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
            $(".alert .close").click(() => $(".alert").hide());
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
                        quiz.ownerId = model.ownerId;
                        api = '/api/quiz/update';
                    }
                    
                    axios.post(api, JSON.stringify(quiz), {
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    }).then(function(response) {
                        if (global.debug) util.logResponse(response);
                        if (response.status === 200) window.location.href = '/Profile';
                    }).catch(function(e) {
                        if (global.debug) util.logResponse(e);
                    });
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
                        this.uploadImage(e.target.files[0])
                            .then(path => question.imagePath = path)
                            .catch(e => {
                                if (e.response.status === 400)
                                    util.openModal(e.response.data);
                            });
                    break;
                    case 'alternative':
                        let altIndex = parseInt($(e.target).attr("index"));
                        question.alternatives[altIndex] = e.target.value;
                    break;
                }

                this.$set(this.questions, i, question);
            },
            changeQuizImg: function(e) {
                this.uploadImage(e.target.files[0])
                    .then(path => $("#quizImgPrev").attr("src", path))
                    .catch(e => {
                        util.logResponse(e);
                    });
            },
            uploadImage: function(file) {
                let formData = new FormData();
                formData.set('image', file);
                
                return new Promise(function(resolve, reject) {
                    axios.post('/api/user/uploadImage', formData, {
                        headers: {
                            'Content-Type': 'multipart/form-data'
                        }
                    }).then(function(response) {
                        if (global.debug) util.logResponse(response);
                        resolve(response.data);
                    }).catch(function(err) {
                        reject(err);
                    });
                });
            },
            questionsToJson: function() {
                let questions = [];
                //TODO: This should be done on the server instead
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
            }
        }
    });
});