// Данные отзывов
const reviews = [
    { text: "Отличный сервис!", author: "Иван, BMW X5" },
    { text: "Автомойка на высшем уровне!", author: "Анна, Toyota Camry" }
];

let currentReview = 0;

function showReview() {
    const reviewText = document.getElementById('review-text');
    const reviewAuthor = document.getElementById('review-author');

    reviewText.textContent = reviews[currentReview].text;
    reviewAuthor.textContent = reviews[currentReview].author;

    currentReview = (currentReview + 1) % reviews.length;
}

// Показываем новый отзыв каждые 5 секунд
setInterval(showReview, 5000);

// Инициализация при загрузке
document.addEventListener('DOMContentLoaded', showReview);