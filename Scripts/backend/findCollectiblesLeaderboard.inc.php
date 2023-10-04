<?php
// findCollectiblesLeaderboard.inc.php

// Constants for response codes
const SUCCESS_CODE = '0';
const QUERY_EMPTY_CODE = '1';
const MISSING_POST_VAR_CODE = '2';
const UNKNOWN_ERROR_CODE = '7';

if (!isset($_POST["code1234"])) {
    echo MISSING_POST_VAR_CODE;
    exit();
}

$unity = true;
$login = true;
require_once '../../keys/storage.php';

try {
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    // First Query: Find Top 10 Players based on Collectibles
    $sql = <<<'SQL'
    SELECT p.id, p.username, COALESCE(LENGTH(ps.collectiblesData) - LENGTH(REPLACE(ps.collectiblesData, '1', '')), 0) as golden_collectibles
    FROM players p
    LEFT JOIN player_stats ps ON p.id = ps.player_id
    ORDER BY golden_collectibles DESC, p.username ASC
    LIMIT 10
    SQL;

    $result = $pdo->query($sql);

    if ($result->rowCount() <= 0) {
        echo QUERY_EMPTY_CODE;
        exit();
    }

    $responseString = SUCCESS_CODE;
    while ($row = $result->fetch(PDO::FETCH_ASSOC)) {
        $responseString .= "\t" . $row["id"] . "\t" . $row["username"] . "\t" . $row["golden_collectibles"];
    }

    // Second Part: Player's Own Stats
    $player_username = $_POST["player_name"]; // Receive this from C#

    $sqlRank = <<<'SQL'
    SELECT COUNT(*) as rank 
    FROM player_stats ps1
    WHERE (COALESCE(LENGTH(ps1.collectiblesData) - LENGTH(REPLACE(ps1.collectiblesData, '1', '')), 0))
        >= (SELECT COALESCE(LENGTH(ps2.collectiblesData) - LENGTH(REPLACE(ps2.collectiblesData, '1', '')), 0) 
            FROM players p2 
            LEFT JOIN player_stats ps2 ON p2.id = ps2.player_id 
            WHERE p2.username = :username)
    SQL;

    $sqlCollectibles = <<<'SQL'
    SELECT COALESCE(LENGTH(ps.collectiblesData) - LENGTH(REPLACE(ps.collectiblesData, '1', '')), 0) as golden_collectibles 
    FROM players p
    LEFT JOIN player_stats ps ON p.id = ps.player_id
    WHERE p.username = :username
    SQL;

    $stmtRank = $pdo->prepare($sqlRank);
    $stmtCollectibles = $pdo->prepare($sqlCollectibles);
    $stmtRank->bindParam(':username', $player_username, PDO::PARAM_STR);
    $stmtCollectibles->bindParam(':username', $player_username, PDO::PARAM_STR);

    $stmtRank->execute();
    $stmtCollectibles->execute();

    $rowRank = $stmtRank->fetch(PDO::FETCH_ASSOC);
    $rowCollectibles = $stmtCollectibles->fetch(PDO::FETCH_ASSOC);

    $responseString .= "\t" . $rowRank["rank"] . "\t" . $player_username . "\t" . $rowCollectibles["golden_collectibles"];
    
    echo $responseString;
    exit();

} catch (PDOException $e) {
    error_log("Database error: " . $e->getMessage());
    echo UNKNOWN_ERROR_CODE;
    exit();
}
?>
