-- ✅ 1. 사용자 정보 테이블
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    password_plain VARCHAR(255),  -- ⚠️ 원본 비번
    mbti VARCHAR(4),
    unique_code VARCHAR(10) UNIQUE NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

select * from users;
ALTER TABLE users
ADD COLUMN password_plain VARCHAR(255);

-- ✅ 2. 사용자 점수 및 랭킹 테이블
CREATE TABLE user_scores (
    user_id INT PRIMARY KEY,
    total_score INT DEFAULT 0,
    user_rank INT DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- ✅ 3. 포인트 적립 내역 테이블
CREATE TABLE point_history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    points INT NOT NULL,
    reason VARCHAR(255),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- ✅ 4. 청소 기록 테이블
CREATE TABLE cleaning_records (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    cleaned_items JSON NOT NULL,
    category VARCHAR(50),
    photo_url VARCHAR(255) NULL,
    cleaned_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

SET @rank = 0;

UPDATE user_scores
JOIN (
    SELECT user_id, total_score, @rank := @rank + 1 AS new_rank
    FROM user_scores
    ORDER BY total_score DESC
) ranked_users ON user_scores.user_id = ranked_users.user_id
SET user_scores.user_rank = ranked_users.new_rank;

-- ✅ 각 테이블의 외래키 CASCADE 적용
ALTER TABLE user_scores
ADD CONSTRAINT fk_user_scores_user_id FOREIGN KEY (user_id) 
REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE point_history
ADD CONSTRAINT fk_point_history_user_id FOREIGN KEY (user_id) 
REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE cleaning_records
ADD CONSTRAINT fk_cleaning_records_user_id FOREIGN KEY (user_id) 
REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE;

-- ✅ 5. 설문 응답 관련
CREATE TABLE survey_responses (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    question_key VARCHAR(255) NOT NULL,  -- 버튼 이름 또는 질문 구분 키
    answer VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_user_question (user_id, question_key)  -- 중복 방지 및 덮어쓰기 위해
);

ALTER TABLE survey_responses
ADD CONSTRAINT fk_survey_responses_user_id FOREIGN KEY (user_id)
REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE;

SELECT * FROM survey_responses;
select * FROM users;
delete from users
where id = 3;

use selterior01db;
SHOW TABLES;