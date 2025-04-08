const express = require("express");
const mysql = require("mysql");
const bcrypt = require("bcrypt");
const jwt = require("jsonwebtoken");
const cors = require("cors");
require("dotenv").config();

const app = express();
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));  // ✅ 이 줄을 추가해야 Unity에서 보내는 form 데이터를 파싱 가능!


// ✅ MySQL 연결
const db = mysql.createConnection({
    host: process.env.DB_HOST || "localhost",
    user: process.env.DB_USER || "selterior01",
    password: process.env.DB_PASS || "0000",
    database: process.env.DB_NAME || "selterior01db"
});

db.connect((err) => {
    if (err) {
        console.error("❌ MySQL 연결 오류:", err);
        process.exit(1);
    }
    console.log("✅ MySQL Connected...");
});

// ✅ 회원가입 API
app.post("/register", async (req, res) => {
    const { username, password, mbti } = req.body;
    if (!username || !password || !mbti) {
        return res.status(400).json({ error: "모든 필드를 입력해주세요." });
    }
    
    try {
        db.query("SELECT id FROM users WHERE username = ?", [username], async (err, results) => {
            if (err) return res.status(500).json({ error: "서버 오류" });
            if (results.length > 0) {
                return res.status(400).json({ error: "이미 존재하는 사용자입니다." });
            }

            const hashedPassword = await bcrypt.hash(password, 10);
            const uniqueCode = Math.random().toString(36).substr(2, 10);
            
            db.query("INSERT INTO users (username, password_hash, password_plain, mbti, unique_code) VALUES (?, ?, ?, ?, ?)",
                [username, hashedPassword, password, mbti, uniqueCode],
                (err, result) => {
                    if (err) return res.status(500).json({ error: "데이터베이스 오류" });

                    db.query("INSERT IGNORE INTO user_scores (user_id, total_score) VALUES (?, 0)", [result.insertId]);
                    res.json({ success: true, message: "회원가입 완료", uniqueCode });
                }
            );
        });
    } catch (error) {
        res.status(500).json({ error: "서버 내부 오류 발생" });
    }
});


// ✅ 로그인 API
app.post("/login", (req, res) => {
    const { username, password } = req.body;
    if (!username || !password) {
        return res.status(400).json({ error: "모든 필드를 입력해주세요." });
    }

    db.query("SELECT * FROM users WHERE username = ?", [username], async (err, results) => {
        if (err) return res.status(500).json({ error: "서버 오류" });
        if (results.length === 0) return res.status(400).json({ error: "사용자를 찾을 수 없습니다." });

        const user = results[0];
        const isMatch = await bcrypt.compare(password, user.password_hash);
        if (!isMatch) return res.status(401).json({ error: "비밀번호가 틀렸습니다." });

        const token = jwt.sign(
            { id: user.id, username: user.username },
            process.env.JWT_SECRET || "default_secret",
            { expiresIn: "1h" }
        );

        // ✅ userId를 응답에 포함!
        res.json({
            success: true,
            message: "로그인 성공!",
            token,
            userId: user.id
        });
    });
});


// ✅ 포인트 지급 및 랭킹 업데이트 API
app.post("/confirm-cleaning", (req, res) => {
    const { userId, cleanedItems } = req.body;
    if (!userId || isNaN(userId) || !Array.isArray(cleanedItems) || cleanedItems.length === 0) {
        return res.status(400).json({ error: "잘못된 요청입니다." });
    }
    const pointsEarned = cleanedItems.length * 10;
    const cleanedAt = new Date().toISOString().slice(0, 19).replace("T", " ");
    
    db.query("SELECT id FROM users WHERE id = ?", [userId], (err, results) => {
        if (err || results.length === 0) return res.status(400).json({ error: "존재하지 않는 사용자입니다." });
        
        db.query("INSERT INTO cleaning_records (user_id, cleaned_items, cleaned_at) VALUES (?, ?, ?)",
            [userId, JSON.stringify(cleanedItems), cleanedAt],
            (err) => {
                if (err) return res.status(500).json({ error: "청소 기록 저장 오류" });
                
                db.query("UPDATE user_scores SET total_score = total_score + ? WHERE user_id = ?", [pointsEarned, userId], (err) => {
                    if (err) return res.status(500).json({ error: "포인트 업데이트 오류" });
                    
                    db.query("SET @rank = 0;", (err) => {
                        if (err) return res.status(500).json({ error: "랭킹 변수 초기화 오류" });
                        
                        db.query(
                            `UPDATE user_scores
                            JOIN (
                                SELECT user_id, total_score, (@rank := @rank + 1) AS new_rank
                                FROM user_scores ORDER BY total_score DESC
                            ) ranked_users ON user_scores.user_id = ranked_users.user_id
                            SET user_scores.user_rank = ranked_users.new_rank;`,
                            (err) => {
                                if (err) return res.status(500).json({ error: "랭킹 업데이트 오류" });
                                res.json({ success: true, message: "청소 완료! 포인트와 랭킹이 업데이트되었습니다.", pointsEarned });
                            }
                        );
                    });
                });
            }
        );
    });
});

// ✅ 랭킹 조회 API
app.get("/ranking", (req, res) => {
    db.query("SELECT u.username, s.total_score, s.user_rank FROM user_scores s JOIN users u ON s.user_id = u.id ORDER BY s.user_rank ASC", 
    (err, results) => {
        if (err) return res.status(500).json({ error: "랭킹 조회 오류" });
        res.json({ success: true, rankings: results });
    });
});

// ✅ 서버 실행
app.listen(3000, '0.0.0.0', () => {
    console.log("🚀 Server running on port 3000");
});

// ✅ 응답 저장 API (업서트)
app.post('/api/survey/submit', (req, res) => {
    const { user_id, question_key, answer } = req.body;
  
    if (!user_id || !question_key || !answer) {
      return res.status(400).json({ message: '필수 값 누락' });
    }
  
    const sql = `
      INSERT INTO survey_responses (user_id, question_key, answer)
      VALUES (?, ?, ?)
      ON DUPLICATE KEY UPDATE
        answer = VALUES(answer),
        updated_at = CURRENT_TIMESTAMP
    `;
  
    db.query(sql, [user_id, question_key, answer], (err, result) => {
      if (err) {
        console.error('응답 저장 오류:', err);
        return res.status(500).json({ message: '서버 오류' });
      }
  
      return res.status(200).json({ message: '응답 저장 완료' });
    });
  });
  