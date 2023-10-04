<?php
// findWinsLeaderBoard.inc.php

const SUCCESS_CODE = '0';
const QUERY_EMPTY_CODE = '1';
const MISSING_POST_VAR_CODE = '2';
const UNKNOWN_ERROR_CODE = '7';

if (!isset($_POST["code1234"]) || !isset($_POST["player_name"])) {
    echo MISSING_POST_VAR_CODE;
    exit();
}

$unity = true;
$login = true;
require_once '../../keys/storage.php';

try {
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

    // Query for the leaderboard data
    $sql = "SELECT p.id, p.username, COALESCE(ps.wins, 0) as wins
            FROM players p
            LEFT JOIN player_stats ps ON p.id = ps.player_id
            ORDER BY wins DESC, p.username ASC
            LIMIT 10";

    $result = $pdo->query($sql);

    $returnString = ($result->rowCount() > 0) ? SUCCESS_CODE : QUERY_EMPTY_CODE;

    while ($row = $result->fetch(PDO::FETCH_ASSOC)) {
        $returnString .= "\t" . $row["id"] . "\t" . $row["username"] . "\t" . $row["wins"];
    }

    // Fetch player's own stats
    $player_username = $_POST["player_name"];

    $sqlRank = "SELECT COUNT(*) as rank 
                FROM player_stats ps1
                WHERE ps1.wins >= (SELECT COALESCE(ps2.wins, 0) FROM players p2 LEFT JOIN player_stats ps2 ON p2.id = ps2.player_id WHERE p2.username = :username)";

    $sqlWins = "SELECT COALESCE(ps.wins, 0) as wins 
                FROM players p
                LEFT JOIN player_stats ps ON p.id = ps.player_id
                WHERE p.username = :username";

    $stmtRank = $pdo->prepare($sqlRank);
    $stmtWins = $pdo->prepare($sqlWins);
    $stmtRank->bindParam(':username', $player_username, PDO::PARAM_STR);
    $stmtWins->bindParam(':username', $player_username, PDO::PARAM_STR);
    $stmtRank->execute();
    $stmtWins->execute();

    $rowRank = $stmtRank->fetch(PDO::FETCH_ASSOC);
    $rowWins = $stmtWins->fetch(PDO::FETCH_ASSOC);

    // Append player's own details
    $returnString .= "\t" . $rowRank["rank"] . "\t" . $player_username . "\t" . $rowWins["wins"];

    echo $returnString;
    exit();

} catch (PDOException $e) {
    error_log("Database error: " . $e->getMessage());
    echo UNKNOWN_ERROR_CODE;
    exit();
}
?>
